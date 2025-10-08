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
                     .Mappings(m => m
                         .Properties(p => p
                             .Text("name", t => t.Analyzer("standard"))
                             .Keyword("genre")
                             .IntegerNumber("id")
                             .DoubleNumber("price")
                             .DoubleNumber("finalPrice")
                             .IntegerNumber("promotionId")
                             .Boolean("hasActivePromotion")
                             .IntegerNumber("popularityScore")
                             .Keyword("tags")
                             .Date("indexedAt")
                         )
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

            logger.LogInformation("Indexed {Success} games successfully, {Failed} failed", successCount, failedCount);

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
                        .Indices(IndexName)
                        .Query(q => q.MatchAll())
                        .Size(3)
                    );

                    logger.LogInformation("Verified indexing: expected {Expected}, actual {Actual}", expectedCount, actualCount);
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
                .Indices(IndexName)
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
                .Indices(IndexName)
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
                .Indices(IndexName)
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
                .Indices(IndexName)
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
                .Indices(IndexName)
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
            logger.LogInformation("SearchWithDSLAsync called with queryParams: {Params}", queryParams);

            if (!queryParams.Any())
                return await GetAllGamesAsync();

            var searchDescriptor = new SearchRequest<GameDocument>(IndexName)
            {
                Size = limit,
                Query = BuildSearchQuery(queryParams)
            };

            var sortField = queryParams.GetValueOrDefault("sort") ?? queryParams.GetValueOrDefault("orderby", "name");
            var sortDirection = queryParams.GetValueOrDefault("order", "asc").ToLowerInvariant();
            var order = sortDirection == "desc" ? SortOrder.Desc : SortOrder.Asc;

            searchDescriptor.Sort = BuildSort(sortField, order);

            var resp = await client.SearchAsync<GameDocument>(searchDescriptor);

            logger.LogInformation("Search response valid: {Valid}, documents count: {Count}", resp.IsValidResponse, resp.Documents.Count);

            if (!resp.IsValidResponse)
            {
                logger.LogError("Search failed: {Error}", resp.ElasticsearchServerError?.Error?.Reason ?? resp.DebugInformation);
                return [];
            }

            return [.. resp.Documents];
        }

        private List<SortOptions> BuildSort(string sortField, SortOrder order)
        {
            var sortOptions = new List<SortOptions>();

            switch (sortField.ToLowerInvariant())
            {
                case "_score":
                case "relevance":
                    sortOptions.Add(new FieldSort { Field = "_score", Order = SortOrder.Desc });
                    break;
                case "name":
                    sortOptions.Add(new FieldSort { Field = "name.keyword", Order = order });
                    break;
                case "price":
                    sortOptions.Add(new FieldSort { Field = "price", Order = order });
                    break;
                case "finalprice":
                    sortOptions.Add(new FieldSort { Field = "finalPrice", Order = order });
                    break;
                case "genre":
                    sortOptions.Add(new FieldSort { Field = "genre", Order = order });
                    break;
                case "popularity":
                    sortOptions.Add(new FieldSort { Field = "popularityScore", Order = order });
                    break;
                case "id":
                    sortOptions.Add(new FieldSort { Field = "id", Order = order });
                    break;
                case "promotion":
                    sortOptions.Add(new FieldSort { Field = "hasActivePromotion", Order = order });
                    break;
                default:

                    sortOptions.Add(new FieldSort { Field = "hasActivePromotion", Order = SortOrder.Desc });
                    sortOptions.Add(new FieldSort { Field = "popularityScore", Order = SortOrder.Desc });
                    sortOptions.Add(new FieldSort { Field = "name.keyword", Order = SortOrder.Asc });
                    break;
            }

            return sortOptions;
        }

        private Query BuildSearchQuery(Dictionary<string, string> queryParams)
        {
            var queries = new List<Query>();

            foreach (var (key, value) in queryParams)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var lowerKey = key.ToLowerInvariant();

                switch (lowerKey)
                {
                    case "q":
                    case "search":
                       
                        var multiMatchQuery = new MultiMatchQuery
                        {
                            Query = value.Trim(),
                            Fields = new[] { "name", "genre", "tags" },
                            Operator = Operator.Or,
                            Fuzziness = new Fuzziness("AUTO")
                        };
                        queries.Add(multiMatchQuery);
                        break;

                    case "name":
                    case "title":
                    case "game":
                        var matchQuery = new MatchQuery
                        {
                            Field = "name",
                            Query = value.Trim(),
                            Fuzziness = new Fuzziness("AUTO")
                        };
                        queries.Add(matchQuery);
                        break;

                    case "genre":
                        var termQuery = new TermQuery
                        {
                            Field = "genre",
                            Value = value.Trim()
                        };
                        queries.Add(termQuery);
                        break;

                    case "pricemin":
                        if (decimal.TryParse(value, out var minPrice))
                        {
                            var rangeMin = new NumberRangeQuery
                            {
                                Field = "price",
                                Gte = (double)minPrice
                            };
                            queries.Add(rangeMin);
                        }
                        break;

                    case "pricemax":
                        if (decimal.TryParse(value, out var maxPrice))
                        {
                            var rangeMax = new NumberRangeQuery
                            {
                                Field = "price",
                                Lte = (double)maxPrice
                            };
                            queries.Add(rangeMax);
                        }
                        break;

                    case "promotion":
                    case "activepromotion":
                    case "haspromotion":
                        if (bool.TryParse(value, out var hasPromo))
                        {
                            var termPromo = new TermQuery
                            {
                                Field = "hasActivePromotion",
                                Value = hasPromo
                            };
                            queries.Add(termPromo);
                        }
                        break;

                    case "tag":
                    case "tags":
                        var termTag = new TermQuery
                        {
                            Field = "tags",
                            Value = value.Trim()
                        };
                        queries.Add(termTag);
                        break;
                }
            }

         
            if (queries.Count == 1)
                return queries[0];

            if (queries.Count > 1)
            {
                return new BoolQuery
                {
                    Must = queries
                };
            }

            return new MatchAllQuery();
        }


        public async Task<IReadOnlyList<GameDocument>> MatchQueryAsync(string field, string query, int limit = 50)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Indices(IndexName)
                .Query(q => q.Match(m => m
                    .Field(field)
                    .Query(query)
                ))
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<IReadOnlyList<GameDocument>> FuzzyQueryAsync(string field, string query, int limit = 50)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Indices(IndexName)
                .Query(q => q.Fuzzy(f => f
                    .Field(field)
                    .Value(query)
                    .Fuzziness(new Fuzziness("AUTO"))
                ))
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<IReadOnlyList<GameDocument>> WildcardQueryAsync(string field, string query, int limit = 50)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Indices(IndexName)
                .Query(q => q.Wildcard(w => w
                    .Field(field)
                    .Value(query)
                ))
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<IReadOnlyList<GameDocument>> MultiMatchQueryAsync(string[] fields, string query, int limit = 50)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Indices(IndexName)
                .Query(q => q.MultiMatch(mm => mm
                    .Fields(fields)
                    .Query(query)
                    .Operator(Operator.Or)
                ))
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<IReadOnlyList<GameDocument>> BoolQueryAsync(Query mustQuery, Query shouldQuery = null, Query mustNotQuery = null, int limit = 50)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Indices(IndexName)
                .Query(q => q
                    .Bool(b =>
                    {
                        if (mustQuery != null)
                            b.Must(mustQuery);
                        if (shouldQuery != null)
                            b.Should(shouldQuery);
                        if (mustNotQuery != null)
                            b.MustNot(mustNotQuery);
                    })
                )
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }

        public async Task<IReadOnlyList<GameDocument>> GoogleStyleSearchAsync(string query, int limit = 50)
        {
            var resp = await client.SearchAsync<GameDocument>(s => s
                .Indices(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.MultiMatch(mm => mm
                                .Fields(new[] { "name", "genre", "tags" })
                                .Query(query)
                                .Operator(Operator.Or)
                            ),
                            sh => sh.Fuzzy(f => f
                                .Field("name")
                                .Value(query)
                                .Fuzziness(new Fuzziness("AUTO"))
                            ),
                            sh => sh.Wildcard(w => w
                                .Field("name")
                                .Value($"*{query}*")
                            )
                        )
                    )
                )
                .Size(limit)
            );

            if (!resp.IsValidResponse)
                return [];

            return [.. resp.Documents];
        }
    }
}