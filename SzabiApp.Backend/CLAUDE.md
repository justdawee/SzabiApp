# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SzabiApp.Backend is an ASP.NET Core 10.0 Web API for a leave/vacation management system. Employees submit leave requests; managers approve or deny them; admins manage users, holidays, and allowances.

## Commands

```bash
# Run locally (with hot reload)
dotnet watch run

# Build
dotnet build SzabiApp.Backend.csproj

# Restore dependencies
dotnet restore SzabiApp.Backend.csproj

# Run tests (once configured)
dotnet test

# Docker
docker compose up
```

Dev ports: HTTP `5062`, HTTPS `7263`. Docker ports: `8080`/`8081`.

OpenAPI JSON: `GET /openapi/v1.json` (Development only). Scalar UI is served alongside it.

## Architecture

**Stack:** ASP.NET Core 10 → Entity Framework Core → PostgreSQL

**Planned layers:**
- Controllers (REST endpoints, OpenAPI docs)
- Services (business logic, leave calculations, validations)
- EF Core entities + DTOs (mapped via AutoMapper)
- PostgreSQL database

**Planned entities:** Users, LeaveRequests, Holidays, WorkSchedules

**Auth:** JWT bearer tokens (stateless, not yet implemented)

## Key Conventions

- **Date/time:** Use NodaTime (`LocalDate`, `LocalDateTime`) for all date/time operations — never `System.DateTime`. NodaTime serialization for System.Text.Json and Npgsql are both configured as dependencies.
- **DTOs:** Separate request/response DTOs; use AutoMapper for entity↔DTO mapping.
- **Error handling:** Use the Result pattern (`Result<T>`) to wrap API responses.
- **Nullable safety:** Nullable reference types are enabled (`<Nullable>enable</Nullable>`); all code must be null-safe.
- **Diagnostics:** Use `CommunityToolkit.Diagnostics` guard helpers instead of manual null/range checks.

## Current State

The project is a skeleton. `Program.cs` has only OpenAPI and HTTPS redirection wired up — no controllers, no EF context, no auth. The next implementation steps are: EF Core models and migrations, JWT auth, CRUD endpoints for leave requests, then the manager/admin workflows.
