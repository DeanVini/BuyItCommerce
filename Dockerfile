FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props BuyItCommerce.sln ./
COPY src/BuyItCommerce.Domain/BuyItCommerce.Domain.csproj src/BuyItCommerce.Domain/
COPY src/BuyItCommerce.Application/BuyItCommerce.Application.csproj src/BuyItCommerce.Application/
COPY src/BuyItCommerce.Infrastructure/BuyItCommerce.Infrastructure.csproj src/BuyItCommerce.Infrastructure/
COPY src/BuyItCommerce.Api/BuyItCommerce.Api.csproj src/BuyItCommerce.Api/
RUN dotnet restore src/BuyItCommerce.Api/BuyItCommerce.Api.csproj

COPY src/ src/
RUN dotnet publish src/BuyItCommerce.Api/BuyItCommerce.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=5 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "BuyItCommerce.Api.dll"]
