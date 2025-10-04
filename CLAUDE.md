# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FIAP Cloud Games (FCG) is a .NET 8 project developed for the FIAP Tech Challenge. It's a microservices-based game management system with Clean Architecture principles, implementing event sourcing, outbox pattern, and dual persistence architecture.

## Architecture

The solution follows Clean Architecture with clear separation of concerns:

- **Api Layer** (`src/Fiap.Api/`): REST API controllers and web configuration
- **Application Layer** (`src/Fiap.Application/`): Business logic and use cases  
- **Domain Layer** (`src/Fiap.Domain/`): Core entities, domain events, and domain logic
- **Infrastructure Layer**: 
  - `src/Fiap.Infra.Data/`: PostgreSQL data persistence with Entity Framework Core
  - `src/Fiap.Infra.CrossCutting.Common/`: Shared infrastructure including MongoDB repositories, caching, and utilities
  - `src/Fiap.Infra.CrossCutting.IoC/`: Dependency injection container
  - `src/Fiap.Infra.Bus/`: Event handlers for domain and integration events
  - `src/Fiap.Infra.HostedService/`: Background services including outbox processor

## Key Architectural Patterns

### Dual Persistence Architecture
The system uses both PostgreSQL (via Entity Framework) and MongoDB for different purposes:
- **PostgreSQL**: Primary transactional data storage with Entity Framework Core
- **MongoDB**: Read-optimized storage accessed via custom repositories in `src/Fiap.Infra.CrossCutting.Common/NoSQL/`

### Event Sourcing and Outbox Pattern
- Domain events are captured and stored in an outbox table
- `OutboxProcessorService` processes events asynchronously via AWS SQS
- Integration events are published to external systems
- Event store implementation available for audit and replay scenarios

### Repository Pattern with Triple Persistence
Each aggregate has multiple repository implementations:
- `IGameRepository` (Entity Framework) - for transactional operations
- `IGameMongoRepository` (MongoDB) - for optimized read operations with aggregation pipelines
- `IElasticSearchService` (Elasticsearch) - for advanced search, aggregations, and recommendations

**Important**: MongoDB repositories use `_id` field for document identification, while Entity Framework uses `Id`. When implementing MongoDB aggregation pipelines, always use `_id` for matching and lookups.

## Development Commands

### Building and Testing

```bash
# Restore packages
dotnet restore

# Build the solution (uses Fiap.slnx)
dotnet build Fiap.slnx

# Run tests
dotnet test

# Run the API 
dotnet run --project src/Fiap.Api/Fiap.Api.csproj
```

**Note**: The project uses `.slnx` format. If build fails due to file locks (common during development), stop the running API first.

### Docker Development

```bash
# Start full development environment
docker-compose up -d

# Build and start just the app
docker-compose up --build app

# Stop all services
docker-compose down
```

The development stack includes:
- PostgreSQL (port 5432) - Primary transactional database
- MongoDB (port 27017) - Read-optimized document store
- Redis (port 6379) - Caching layer
- Elasticsearch (port 9201) - Search engine and document indexing
- Kibana (port 5601) - Elasticsearch data visualization and management
- Filebeat - Log shipping to Elasticsearch
- Grafana (port 3000, admin:admin123) - Observability dashboard
- Jaeger (port 16686) - Distributed tracing
- Prometheus (port 9090) - Metrics collection
- SonarQube (port 9001) - Code quality analysis
- KurrentDB (port 2113) - Event store database
- Main API (port 5000)

### Code Quality

```bash
# Run SonarQube analysis (requires SONAR_TOKEN in .env)
sonar-analyze.bat

# Or run analysis via docker-compose
docker-compose up sonarqube
```

The project uses GitHub Actions for CI/CD with automated validation on PRs to `main` and `dev` branches.

## Key Patterns and Conventions

### Service Layer Architecture
- Application services orchestrate business logic and handle caching
- Services use both Entity Framework and MongoDB repositories strategically
- Redis caching implemented via `ICacheService` with tag-based invalidation
- Cache keys follow pattern: `{EntityPrefix}Id:{id}` (e.g., `Games:Id:14`)

### Event-Driven Architecture
- Domain events trigger integration events via outbox pattern
- AWS SQS for reliable message delivery between bounded contexts
- Event handlers in `src/Fiap.Infra.Bus/Handlers/` process domain events
- Background service `OutboxProcessorService` ensures eventual consistency

### Data Access Patterns
- **Write operations**: Use Entity Framework repositories with PostgreSQL
- **Read operations**: Prefer Elasticsearch for search/aggregations, MongoDB for complex queries
- **Search and recommendations**: Use Elasticsearch with intelligent fallback to MongoDB
- **Aggregation queries**: Use MongoDB aggregation pipelines with `$lookup` for joins
- **Caching**: Redis for frequently accessed data with automatic invalidation
- **Background sync**: `DataSyncHostedService` synchronizes PostgreSQL → MongoDB + Elasticsearch every 5 minutes

### Dependency Injection
- Centralized service registration in `NativeInjector`
- Separate registration methods for different concerns (MongoDB, Redis, ServiceBus, etc.)
- Configuration binding with validation and startup validation

## Development Guidelines

### Working with MongoDB Repositories
When implementing MongoDB repository methods that need to join with related entities:

```csharp
// Use _id for document matching, not Id
new("$match", new BsonDocument("_id", BsonValue.Create(id)))

// Use _id for foreign field lookups
new("$lookup", new BsonDocument
{
    { "from", "promotions" },
    { "localField", "PromotionId" },
    { "foreignField", "_id" },  // MongoDB uses _id
    { "as", "promotionArray" }
})

// Handle null relationships properly with conditional logic
new("$addFields", new BsonDocument
{
    { "Promotion", new BsonDocument("$cond", new BsonDocument
        {
            { "if", new BsonDocument("$eq", new BsonArray { new BsonDocument("$size", "$promotionArray"), 0 }) },
            { "then", BsonNull.Value },
            { "else", new BsonDocument("$arrayElemAt", new BsonArray { "$promotionArray", 0 }) }
        })
    }
})
```

### Cache Management
- Always invalidate related cache keys when entities change
- Use `EnumCacheTags` constants for consistent cache key naming
- Remove cache entries before fetching fresh data to avoid stale cache

### Event Handling
- Domain events are automatically persisted to outbox table during entity saves
- Integration events should be immutable records
- Event handlers should be idempotent

## Environment Configuration

- Development environment uses `appsettings.Development.json`
- Production configuration loaded from AWS Systems Manager (`/fiap/prod` path)
- Environment variables can override configuration values
- User secrets supported in DEBUG mode

### Required Configuration Sections
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PostgreSQL connection string",
    "Redis": "Redis connection string",
    "MongoConnection": "MongoDB connection string"
  },
  "MongoDb": {
    "ConnectionString": "MongoDB connection string",
    "Database": "Database name"
  },
  "ServiceBus": {
    "AccessKey": "AWS Access Key",
    "SecretKey": "AWS Secret Key",
    "Region": "AWS Region",
    "FCGQueueName": "SQS Queue Name"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  }
}
```

## Elasticsearch Integration

The system includes comprehensive Elasticsearch integration for advanced search and analytics:

### Key Features
- **Automatic indexing**: New games are indexed in real-time via `GameCreatedHandler`
- **Background synchronization**: `DataSyncHostedService` syncs PostgreSQL seed data every 5 minutes
- **Intelligent search**: Priority order is Elasticsearch → MongoDB → PostgreSQL
- **Advanced aggregations**: Popular games and personalized recommendations
- **Resilient fallbacks**: Automatic fallback to MongoDB/PostgreSQL if Elasticsearch is unavailable

### Important Endpoints
```
GET /api/v1/games                     - All games with Elasticsearch priority
GET /api/v1/games/{id}                - Single game by ID with Elasticsearch priority
GET /api/v1/games/popular             - Popular games via Elasticsearch aggregations
GET /api/v1/games/recommendations/user/{userId} - Personalized recommendations
POST /api/v1/games                    - Create new game (auto-indexed)
```

**Note**: Refer to `ELASTICSEARCH.md` for detailed documentation on the Elasticsearch integration, including setup, troubleshooting, and performance metrics.