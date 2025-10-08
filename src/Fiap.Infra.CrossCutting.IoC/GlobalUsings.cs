global using Fiap.Application.Games.Services;
global using Fiap.Application.Promotions.Services;

global using Rebus.Routing.TypeBased;

global using Fiap.Domain.GameAggregate;
global using Fiap.Domain.OutboxAggregate;
global using Fiap.Domain.PromotionAggregate;
global using Fiap.Domain.SeedWork;

global using Fiap.Infra.CacheService.Services;
global using Fiap.Infra.Data;
global using Fiap.Infra.Data.Repositories;
global using Fiap.Infra.Utils;
global using Fiap.Infra.CrossCutting.Common.Elastic.Services;
global using Fiap.Infra.MongoDb;
global using Fiap.Infra.MongoDb.Repositories;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Hybrid;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

global using OpenTelemetry.Exporter;
global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;
global using Rebus.Config;
global using Serilog;
global using Serilog.Sinks.Grafana.Loki;
global using StackExchange.Redis;
global using Elastic.Clients.Elasticsearch;
global using Elastic.Transport;
global using Elastic.Clients.Elasticsearch.Mapping;

global using MongoDB.Driver;

global using System.Diagnostics.CodeAnalysis;
global using EventStore.Client;
global using Fiap.Domain.Common.Events;
global using Fiap.Infra.Bus.Handlers;
global using Fiap.Infra.CrossCutting.Common.Http.CRM;
global using Refit;