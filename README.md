# CarParking API

Simple ASP.NET Core Web API for car park management.

## Tech Stack

- .NET 10
- ASP.NET Core Minimal APIs
- PostgreSQL 16
- Entity Framework Core (Npgsql provider)
- FluentValidation

## Project Structure

- CarParking.Api: API, endpoints, EF Core data layer, validators, services
- tests/CarParking.Api.Tests: xUnit test project covering validators, parking charge calculation, services, and minimal API endpoint handlers
- scripts/initial_migration.sql: schema script used to initialize PostgreSQL in Docker

## Tests

The test project uses xUnit with in-memory EF Core and Moq to cover the main application slices:

- request validators
- parking charge calculation
- parking service behavior
- minimal API endpoint handlers

Current maintained API coverage is around 76% after excluding generated OpenAPI output and EF migration files from coverage reports.

## Prerequisites

- .NET SDK 10
- Docker Desktop (for Docker run mode)

## Setup and Run Locally

### Option A: Run with Docker Compose (recommended)

From repository root:

```bash
docker compose up --build
```

This starts:

- API at http://localhost:8080
- PostgreSQL at localhost:5432

Database bootstrap:

- scripts/initial_migration.sql is mounted into the PostgreSQL init folder and executed on first container startup (when the postgres_data volume is empty).

Database seed:

- scripts/seed_data.sql - to apply manually after connecting to the database.

Stop:

```bash
docker compose down
```

Reset database volume and re-run init script:

```bash
docker compose down -v
docker compose up --build
```

### Option B: Run API from local machine + local PostgreSQL

1. Ensure a PostgreSQL instance is running.
2. Create database and apply script:

```bash
psql -h localhost -U postgres -d postgres -c "CREATE DATABASE \"CarParking\";"
psql -h localhost -U postgres -d CarParking -f scripts/initial_migration.sql
```

3. Verify/update connection string in CarParking.Api/appsettings.Development.json under ConnectionStrings:Postgres.
4. Run API:

```bash
dotnet run -lp "https" --project CarParking.Api/CarParking.Api.csproj
```

By default, Development launch profile uses:

- https://localhost:7135; http://localhost:5105

## API Endpoints

- GET /parking
- POST /parking
- POST /parking/exit

OpenAPI/Swagger UI:

- Docker: http://localhost:8080/swagger
- Local run: https://localhost:7135/swagger

## Assumptions

- Vehicle registration is unique per active parking session.
- One parking space can have only one active session at a time.
- Active session is represented by TimeOut = null.
- The SQL bootstrap script is intended for local/dev setup.
- All timestamps are stored as timestamp with time zone.

## AI Assistance

GitHub Copilot AI agent was used only for test generation and README maintenance.
