# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fiap.Api/Fiap.Api.csproj", "src/Fiap.Api/"]
COPY ["src/Fiap.Application/Fiap.Application.csproj", "src/Fiap.Application/"]
COPY ["src/Fiap.Domain/Fiap.Domain.csproj", "src/Fiap.Domain/"]
COPY ["src/Fiap.Infra.CrossCutting.Common/Fiap.Infra.CrossCutting.Common.csproj", "src/Fiap.Infra.CrossCutting.Common/"]
COPY ["src/Fiap.Infra.CrossCutting.IoC/Fiap.Infra.CrossCutting.IoC.csproj", "src/Fiap.Infra.CrossCutting.IoC/"]
COPY ["src/Fiap.Infra.Bus/Fiap.Infra.Bus.csproj", "src/Fiap.Infra.Bus/"]
COPY ["src/Fiap.Infra.Data/Fiap.Infra.Data.csproj", "src/Fiap.Infra.Data/"]
COPY ["src/Fiap.Infra.HostedService/Fiap.Infra.HostedService.csproj", "src/Fiap.Infra.HostedService/"]
RUN dotnet restore "./src/Fiap.Api/Fiap.Api.csproj"
COPY . .
WORKDIR "/src/src/Fiap.Api"
RUN dotnet build "./Fiap.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fiap.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.Api.dll"]