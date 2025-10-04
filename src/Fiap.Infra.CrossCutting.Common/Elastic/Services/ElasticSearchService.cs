namespace Fiap.Infra.CrossCutting.Common.Elastic.Services
{
    public class ElasticSearchService(ElasticsearchClient client, ILogger<ElasticSearchService> logger) : IElasticSearchService
    {
        private const string IndexName = "games";

        public async Task<bool> IndexGamesAsync(IEnumerable<GameDocument> games)
        {
            if (games is null || !games.Any())
                return false;

            var gamesList = games.ToList();

            var indexExistsResp = await client.Indices.ExistsAsync(IndexName);
            
            if (!indexExistsResp.Exists)
            {
                var createIndexResp = await client.Indices.CreateAsync(IndexName, c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0)
                        .RefreshInterval(TimeSpan.FromSeconds(1))
                    )
                );

                if (!createIndexResp.IsValidResponse)
                    return false;

                await Task.Delay(3000);
            }

            var successCount = 0;
            var failedCount = 0;
            
            for (int i = 0; i < gamesList.Count; i++)
            {
                var game = gamesList[i];
               
                var singleResp = await client.IndexAsync(game, idx => idx
                    .Index(IndexName)
                    .Id(game.Id.ToString())
                    .Refresh(Refresh.True) 
                );
                
                if (singleResp.IsValidResponse)
                    successCount++;
                else
                    failedCount++;
            }

            if (successCount > 0)
            {
                await client.Indices.RefreshAsync(IndexName);
                return await VerifyIndexingSimple(successCount);
            }

            return false;
        }

        private async Task<bool> VerifyIndexingSimple(int expectedCount)
        {
            await Task.Delay(2000);
            
            var countResp = await client.CountAsync<GameDocument>(c => c
                .Indices(IndexName)
            );

            if (countResp.IsValidResponse)
            {
                var actualCount = (int)countResp.Count;
                
                if (actualCount >= expectedCount)
                {
                    var sampleResp = await client.SearchAsync<GameDocument>(s => s
                        .Index(IndexName)
                        .Query(q => q.MatchAll())
                        .Size(3)
                    );
                   
                    return true;
                }
                else
                {
                    logger.LogWarning("Expected {ExpectedCount} documents but found {ActualCount}", expectedCount, actualCount);
                    return actualCount > 0; 
                }
            }
            
            return false;
        }

        public async Task<IReadOnlyList<GameDocument>> GetAllGamesAsync()
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Index(IndexName)
                .Query(q => q.MatchAll())
                .Sort(so => so
                    .Field(f => f.Field("popularityScore").Order(SortOrder.Desc))
                    .Field(f => f.Field("id").Order(SortOrder.Asc))
                )
                .Size(1000)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<GameDocument?> GetGameByIdAsync(int id)
        {
            var getResp = await client.GetAsync<GameDocument>(id.ToString(), idx => idx.Index(IndexName));
            
            if (getResp.IsValidResponse && getResp.Found)
                return getResp.Source;

            var searchResp = await client.SearchAsync<GameDocument>(s => s
                .Index(IndexName)
                .Query(q => q.Term(t => t.Field("id").Value(id)))
                .Size(1)
            );

            if (searchResp.IsValidResponse)
                return searchResp.Documents.FirstOrDefault();
            
            return null;
        }

        public async Task<IReadOnlyList<GameDocument>> GetRecommendationsByGenreAsync(string genre, int userId, int limit = 10)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m.Term(t => t.Field("genre").Value(genre)))
                        .Should(sh => sh
                            .Term(t => t.Field("hasActivePromotion").Value(true))
                        )
                    )
                )
                .Sort(so => so
                    .Field(f => f.Field("hasActivePromotion").Order(SortOrder.Desc))
                    .Field(f => f.Field("popularityScore").Order(SortOrder.Desc))
                    .Field(f => f.Field("id").Order(SortOrder.Asc))
                )
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<IReadOnlyList<GameDocument>> GetMostPopularGamesAsync(int limit = 10)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Index(IndexName)
                .Query(q => q.MatchAll())
                .Sort(so => so
                    .Field(f => f.Field("popularityScore").Order(SortOrder.Desc))
                    .Field(f => f.Field("hasActivePromotion").Order(SortOrder.Desc))
                    .Field(f => f.Field("price").Order(SortOrder.Asc))
                )
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<bool> UpdateGamePopularityAsync(int gameId, int popularityScore)
        {
            var resp = await client.UpdateAsync<GameDocument, object>(gameId.ToString(), IndexName, u => u
                .Doc(new { popularityScore })
                .DocAsUpsert(false)
            );

            return resp.IsValidResponse;
        }

        public async Task<bool> BulkUpdatePopularityAsync(Dictionary<int, int> gamePopularityScores)
        {
            var allSuccess = true;
            
            foreach (var (gameId, score) in gamePopularityScores)
            {
                var success = await UpdateGamePopularityAsync(gameId, score);
                if (!success)
                    allSuccess = false;
            }

            return allSuccess;
        }

        public async Task<IReadOnlyList<GameDocument>> SearchGamesAsync(string? searchTerm = null, string? genre = null, int limit = 50)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Index(IndexName)
                .Query(q => q.MatchAll())
                .Sort(so => so
                    .Field(f => f.Field("hasActivePromotion").Order(SortOrder.Desc))
                    .Field(f => f.Field("popularityScore").Order(SortOrder.Desc))
                    .Field(f => f.Field("price").Order(SortOrder.Asc))
                )
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            var results = resp.Documents.ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                results = results.Where(g => 
                    g.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    g.Genre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                results = results.Where(g => 
                    g.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return results.AsReadOnly();
        }

        public async Task<IReadOnlyList<GameDocument>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idsList = ids.ToList();

            if (!idsList.Any())
                return [];

            var allGames = await GetAllGamesAsync();
            var filteredGames = allGames.Where(g => idsList.Contains(g.Id)).ToList();

            return filteredGames.AsReadOnly();
        }

        public async Task<IReadOnlyList<GameDocument>> SearchWithDSLAsync(Dictionary<string, string> queryParams, int limit = 50)
        {
            if (!queryParams.Any())
                return await GetAllGamesAsync();

            var allGames = await GetAllGamesAsync();
            
            if (!allGames.Any())
                return [];

            var filteredGames = allGames.AsEnumerable();

            foreach (var (key, value) in queryParams)
            {
                if (string.IsNullOrWhiteSpace(value)) 
                    continue;

                var lowerKey = key.ToLowerInvariant();
                var lowerValue = value.ToLowerInvariant();

                filteredGames = lowerKey switch
                {
                    "genre" => filteredGames.Where(g => g.Genre.Equals(value, StringComparison.OrdinalIgnoreCase)),
                    "id" => int.TryParse(value, out var id) ? filteredGames.Where(g => g.Id == id) : filteredGames,
                    "promotionid" => int.TryParse(value, out var promId) ? filteredGames.Where(g => g.PromotionId == promId) : filteredGames,
                    
                    "name" => filteredGames.Where(g => g.Name.Contains(value, StringComparison.OrdinalIgnoreCase)),
                    "title" => filteredGames.Where(g => g.Name.Contains(value, StringComparison.OrdinalIgnoreCase)),
                    "game" => filteredGames.Where(g => g.Name.Contains(value, StringComparison.OrdinalIgnoreCase)),

                    "price" => FilterByPrice(filteredGames, value),
                    "pricemin" => decimal.TryParse(value, out var minPrice) ? filteredGames.Where(g => g.Price >= minPrice) : filteredGames,
                    "pricemax" => decimal.TryParse(value, out var maxPrice) ? filteredGames.Where(g => g.Price <= maxPrice) : filteredGames,
                    "finalprice" => decimal.TryParse(value, out var finalPrice) ? filteredGames.Where(g => g.FinalPrice <= finalPrice) : filteredGames,
                    
                    "promotion" => bool.TryParse(value, out var hasPromo) ? filteredGames.Where(g => g.HasActivePromotion == hasPromo) : filteredGames,
                    "activepromotion" => bool.TryParse(value, out var activePromo) ? filteredGames.Where(g => g.HasActivePromotion == activePromo) : filteredGames,
                    "haspromotion" => bool.TryParse(value, out var hasActivePromo) ? filteredGames.Where(g => g.HasActivePromotion == hasActivePromo) : filteredGames,

                    "popularity" => int.TryParse(value, out var popularity) ? filteredGames.Where(g => g.PopularityScore >= popularity) : filteredGames,
                    "popular" => filteredGames.OrderByDescending(g => g.PopularityScore),

                    "tag" => filteredGames.Where(g => g.Tags?.Any(t => t.Contains(lowerValue, StringComparison.OrdinalIgnoreCase)) == true),
                    "tags" => filteredGames.Where(g => g.Tags?.Any(t => t.Contains(lowerValue, StringComparison.OrdinalIgnoreCase)) == true),

                    "search" => filteredGames.Where(g => 
                        g.Name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        g.Genre.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        g.Tags?.Any(t => t.Contains(lowerValue, StringComparison.OrdinalIgnoreCase)) == true
                    ),
                    "q" => filteredGames.Where(g => 
                        g.Name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        g.Genre.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        g.Tags?.Any(t => t.Contains(lowerValue, StringComparison.OrdinalIgnoreCase)) == true
                    ),

                    _ => filteredGames.Where(g => g.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                };
            }

            if (queryParams.ContainsKey("sort") || queryParams.ContainsKey("orderby"))
            {
                var sortField = queryParams.GetValueOrDefault("sort") ?? queryParams.GetValueOrDefault("orderby", "name");
                var sortDirection = queryParams.GetValueOrDefault("order", "asc").ToLowerInvariant();

                filteredGames = ApplySorting(filteredGames, sortField, sortDirection);
            }
            else
            {
                filteredGames = filteredGames
                    .OrderByDescending(g => g.HasActivePromotion)
                    .ThenByDescending(g => g.PopularityScore)
                    .ThenBy(g => g.Name);
            }

            var results = filteredGames.Take(limit).ToList();
            return results.AsReadOnly();
        }

        private IEnumerable<GameDocument> FilterByPrice(IEnumerable<GameDocument> games, string priceValue)
        {
            if (priceValue.StartsWith(">="))
                return decimal.TryParse(priceValue[2..], out var price) ? games.Where(g => g.Price >= price) : games;
            
            if (priceValue.StartsWith("<="))
                return decimal.TryParse(priceValue[2..], out var price) ? games.Where(g => g.Price <= price) : games;
            
            if (priceValue.StartsWith(">"))
                return decimal.TryParse(priceValue[1..], out var price) ? games.Where(g => g.Price > price) : games;
            
            if (priceValue.StartsWith("<"))
                return decimal.TryParse(priceValue[1..], out var price) ? games.Where(g => g.Price < price) : games;
            
            if (priceValue.Contains("-"))
            {
                var parts = priceValue.Split('-');
                if (parts.Length == 2 && decimal.TryParse(parts[0], out var min) && decimal.TryParse(parts[1], out var max))
                    return games.Where(g => g.Price >= min && g.Price <= max);
            }

            return decimal.TryParse(priceValue, out var exactPrice) ? games.Where(g => g.Price == exactPrice) : games;
        }

        private IEnumerable<GameDocument> ApplySorting(IEnumerable<GameDocument> games, string sortField, string direction)
        {
            var isDescending = direction == "desc" || direction == "descending";

            return sortField.ToLowerInvariant() switch
            {
                "name" => isDescending ? games.OrderByDescending(g => g.Name) : games.OrderBy(g => g.Name),
                "price" => isDescending ? games.OrderByDescending(g => g.Price) : games.OrderBy(g => g.Price),
                "finalprice" => isDescending ? games.OrderByDescending(g => g.FinalPrice) : games.OrderBy(g => g.FinalPrice),
                "genre" => isDescending ? games.OrderByDescending(g => g.Genre) : games.OrderBy(g => g.Genre),
                "popularity" => isDescending ? games.OrderByDescending(g => g.PopularityScore) : games.OrderBy(g => g.PopularityScore),
                "id" => isDescending ? games.OrderByDescending(g => g.Id) : games.OrderBy(g => g.Id),
                "promotion" => isDescending ? games.OrderByDescending(g => g.HasActivePromotion) : games.OrderBy(g => g.HasActivePromotion),
                _ => isDescending ? games.OrderByDescending(g => g.Name) : games.OrderBy(g => g.Name)
            };
        }
    }
}