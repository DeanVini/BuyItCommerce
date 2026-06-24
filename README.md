# BuyItCommerce

API RESTful para gerenciamento de pedidos de e-commerce, construída em Clean Architecture
com CQRS e persistência poliglota: **SQL Server** como banco de escrita (fonte da verdade,
transacional) e **MongoDB** como banco de leitura (desnormalizado, otimizado para consulta).
A criação de pedidos é idempotente: repetir a mesma requisição nunca duplica uma compra.

A documentação de arquitetura completa está em [docs/arquitetura](docs/arquitetura).

## Stack

.NET 9 · Minimal APIs · MediatR (CQRS) · Entity Framework Core (SQL Server) · MongoDB ·
FluentValidation · Serilog · OpenAPI + Scalar · xUnit + FluentAssertions + Moq · Docker

## Arquitetura

Quatro projetos seguindo a regra de dependência da Clean Architecture
(`Api → Infrastructure → Application → Domain`):

```
src/
├── BuyItCommerce.Domain          # Agregado Order, value objects, regras de transição
├── BuyItCommerce.Application      # CQRS (commands/queries), behaviors, contratos
├── BuyItCommerce.Infrastructure  # EF Core (SQL), MongoDB, outbox, cache, config
└── BuyItCommerce.Api             # Minimal APIs, Problem Details, OpenAPI/Scalar, health
tests/
└── BuyItCommerce.Tests           # Testes de unidade
```

A sincronização entre escrita (SQL Server) e leitura (MongoDB) usa o padrão **Outbox**: após
o commit, um `Channel<Guid>` in-memory acorda um `BackgroundService` que projeta o pedido no
MongoDB (com polling periódico como rede de segurança).

## Pré-requisitos

- [.NET SDK 9](https://dotnet.microsoft.com/download) (para rodar/testar localmente)
- [Docker](https://www.docker.com/) (para subir a stack completa)

## Como rodar

### Opção 1 — Docker Compose (stack completa)

Sobe API + SQL Server + MongoDB de uma vez. As migrations são aplicadas automaticamente no
startup da API.

```bash
cp .env.example .env   # ajuste SA_PASSWORD se quiser
docker compose up --build
```

A API fica disponível em `http://localhost:8080`.

### Opção 2 — Local (Visual Studio / dotnet)

Suba apenas os bancos via Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
docker run -p 27017:27017 -d mongo:7
```

Depois rode a API (as connection strings padrão em `appsettings.json` já apontam para
`localhost`):

```bash
dotnet run --project src/BuyItCommerce.Api
```

No Visual Studio: defina `BuyItCommerce.Api` como projeto de inicialização e pressione F5.

### Endpoints úteis

| Recurso | URL (Docker) | URL (local/dotnet) |
|---|---|---|
| Documentação (Scalar) | `http://localhost:8080/scalar/v1` | `http://localhost:5290/scalar/v1` |
| OpenAPI (JSON) | `http://localhost:8080/openapi/v1.json` | `http://localhost:5290/openapi/v1.json` |
| Health check | `http://localhost:8080/health` | `http://localhost:5290/health` |
| Pedidos | `http://localhost:8080/api/v1/orders` | `http://localhost:5290/api/v1/orders` |

A criação de pedido (`POST /api/v1/orders`) exige o header `Idempotency-Key`.

## Testes

Os testes de unidade não dependem de banco de dados:

```bash
dotnet test
```

No Visual Studio: **Test → Test Explorer → Run All Tests**.
