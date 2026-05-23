# SadathEMS Copilot Instructions

## Architecture Philosophy

- Treat this solution as a **modular business application** built on shared building blocks.
- Keep changes **inside the correct module and layer**. Prefer small, local changes over cross-cutting edits.
- Respect **dependency direction**: Domain -> Application -> Infrastructure/Endpoints at the module level. Do not introduce outward dependencies from Domain to infrastructure or UI concerns.
- Use **BuildingBlocks** projects only for reusable cross-module primitives, abstractions, and infrastructure that are truly shared.

## Solution Shape

- `src/BuildingBlocks`
  - `BuildingBlocks.Core`: shared core abstractions and contracts.
  - `BuildingBlocks.Infrastructure`: shared infrastructure concerns.
  - `BuildingBlocks.Workflow`: workflow abstractions/services.
  - `BuildingBlocks.UI`: shared UI building blocks.
- `src/Modules/<ModuleName>`
  - `<Module>.Domain`: domain entities, value objects, domain rules.
  - `<Module>.Application`: use cases, commands/queries, handlers, orchestration.
  - `<Module>.Infrastructure`: persistence, external integrations, implementations.
  - `<Module>.Endpoints`: minimal API endpoint registration and module composition.
- `src/Host/App.ApiHost`
  - Composition root for backend services.
  - Registers modules and maps their endpoints.
- `src/Frontends`
  - `App.Web`: Blazor web frontend.
  - `App.Mobile`: .NET MAUI frontend.
  - `App.SharedUI`: shared Razor UI reused by web/mobile.

## Layering Rules

- **Domain**
  - Contains business concepts and rules only.
  - Must not depend on ASP.NET Core, EF Core, UI, or infrastructure details.
  - May depend on `BuildingBlocks.Core` when needed.
- **Application**
  - Coordinates use cases.
  - Depends on Domain and shared abstractions.
  - Define interfaces here when infrastructure will implement them.
- **Infrastructure**
  - Implements persistence and external service integrations.
  - Depends on Application and shared infrastructure packages.
  - Keep framework-specific code here when possible.
- **Endpoints**
  - Expose module features through minimal APIs.
  - Keep endpoints thin: validate/map request, call application handler, return result.
  - Register each module via extension methods such as `Add{Module}Module()` and `Map{Module}Endpoints()`.

## Module Boundaries

- Prefer **module isolation**.
- Do not reference another module's Infrastructure or Endpoints directly.
- Avoid leaking one module's internal types into another module.
- If cross-module collaboration is needed, use shared abstractions, contracts, or explicit application-level integration patterns.

## API and Composition Guidance

- `App.ApiHost` should stay a thin composition root.
- Add new backend capabilities by:
  1. implementing domain/application/infrastructure inside the target module,
  2. exposing them from the module's Endpoints project,
  3. wiring the module into `App.ApiHost`.
- Prefer minimal APIs for module HTTP surfaces.
- In this modular monolith, modules run in the same process and should normally communicate **in-process** through application contracts and dependency injection, not HTTP.
- Use `App.ApiHost` as the primary HTTP surface for **external clients**, **mobile clients**, and any consumer running in a separate process.
- Avoid calling a module's HTTP endpoints from another module inside the same process unless there is a deliberate architectural reason.

## Frontend and Integration Guidance

- `App.Web` is a Blazor web frontend and may use in-process module services where appropriate.
- `App.Mobile` is a **.NET MAUI Blazor Hybrid** frontend.
- Prefer shared Razor UI in `App.SharedUI` when UI can be reused between `App.Web` and `App.Mobile`.
- For clients in a different process, prefer HTTP calls through `App.ApiHost`.
- For browser cookie-based authentication flows, keep the authentication flow aligned with the hosting app requirements.

## Frontend Guidance

- Prefer **Blazor** patterns for web UI work.
- Prefer **.NET MAUI** patterns for mobile work.
- Reuse UI in `App.SharedUI` when it can be shared between `App.Web` and `App.Mobile`.
- Do not suggest Xamarin.Forms patterns or APIs.

## Mapping Guidance

- Use **AutoMapper** as the standard object-mapping approach across modules.
- Keep mapping contracts in the **Application** layer and shared mapping infrastructure in `BuildingBlocks`.
- Use `BuildingBlocks.Core/Mapping/IMapFrom<T>` for DTOs and models that declare mappings.
- Register module mappings from Infrastructure using `services.AddModuleMappings(typeof(SomeDto).Assembly)`.
- Do not place AutoMapper configuration in Domain.
- Prefer module-local DTOs/view models over exposing EF or infrastructure types directly.

## Coding Guidance for Copilot

- Follow existing naming and project conventions.
- Keep new code consistent with the current .NET 10 solution style.
- Make minimal changes that fit the existing module structure.
- Do not move logic into the host or frontend that belongs in a module layer.
- Do not place business rules in endpoints or UI components.
- Prefer constructor injection and explicit abstractions over service location.
- When adding a feature, first decide which module owns it, then place code in the correct layer.

## When Extending the Solution

- New business capability -> add it to the owning module.
- New reusable primitive used by many modules -> consider `BuildingBlocks`.
- New HTTP route -> add it in the module's Endpoints project, not directly in `App.ApiHost` unless it is host-only behavior.
- New UI shared by web and mobile -> prefer `App.SharedUI`.
- New cross-module mapping convention or reusable mapping primitive -> place it in `BuildingBlocks`.
