global using Fiap.Api.Controllers;
global using Fiap.Application.Common;
global using Fiap.Domain.SeedWork;
global using Microsoft.AspNetCore.Mvc;
global using Moq;

global using Fiap.Application.Games.Models.Request;
global using Fiap.Application.Games.Models.Response;
global using Fiap.Application.Games.Services;

global using Fiap.Application.Promotions.Models.Request;
global using Fiap.Application.Promotions.Models.Response;
global using Fiap.Application.Promotions.Services;
global using System.Text.Json;
global using System.Linq.Expressions;

global using Fiap.Domain.GameAggregate;
global using Fiap.Domain.SeedWork.Exceptions;
global using Fiap.Domain.SeedWork.Enums;
global using Fiap.Domain.GameAggregate.Events;
global using Fiap.Domain.Common.Events;
global using Microsoft.Extensions.Logging;
global using Fiap.Infra.CacheService.Services;

global using Fiap.Domain.Common.ValueObjects;
global using Fiap.Domain.PromotionAggregate;
global using static Fiap.Domain.SeedWork.NotificationModel;
global using Fiap.Domain.OutboxAggregate;