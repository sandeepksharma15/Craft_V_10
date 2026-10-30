# Craft V10 architecture review → V11 direction

Reviewed 30 October 2026 against [Craft_V_10 commit dc9c28f](https://github.com/sandeepksharma15/Craft_V_10/tree/dc9c28f9da9868f161ebfe2f76f0678f0876d66a). Scope: all project manifests, source layout and selected implementation paths; **not** a correctness/security audit or a V10 build/test certification. V11 baseline: `35fe3de` (including the subsequent auditing namespace reorganization).

**Recommendation:** retain the breadth, improve the boundaries. Build independently usable libraries with optional integrations and starter applications; do not require a complete Craft application stack to consume one feature. The following package names and moves are proposals, not approved implementation commitments.

## Existing projects and purpose

| V10 project | Purpose observed in source |
|---|---|
| Craft.Core | Shared service results, pagination, current-user/tenant contracts and DI lifetime markers. Also contains EF context and HTTP response concerns, so it is not currently infrastructure-neutral. |
| Craft.Domain | Entity/value-object/aggregate bases, domain events, capability interfaces, validation resources and exceptions. Also contains DTO/view-model bases. |
| Craft.Extensions | General extension methods for collections, reflection, strings and expressions, alongside EF, HTTP and DI helpers. |
| Craft.Utilities | Reusable builders, timing, retry, password, file and text helpers. Also includes document extraction dependencies and KeySafe encryption. |
| Craft.Data | EF context bases and features for soft delete, auditing, concurrency, versioning, tenancy and Identity. Also handles provider setup, connection strings, migrations, seeding and health checks. |
| Craft.Repositories | Read/write repository contracts and implementations over EF-facing context contracts. Includes fake repository implementations. |
| Craft.QuerySpec | Query construction, filtering, projection, sorting, pagination, serialization and EF evaluation. Also implements repositories, controllers and HTTP services. |
| Craft.Cache | Memory/Redis/null caching providers, key generation, invalidation and cache statistics. |
| Craft.Configuration | Configuration loading, validation and encrypted values. Azure/AWS provider classes are placeholders, not working cloud integrations. |
| Craft.Emails | Mail delivery, SMTP/mock providers, Razor templates and an in-memory background queue. Also exposes email controllers. |
| Craft.Files | File upload/storage services, local storage and access tokens. Its package also references Blazor components. |
| Craft.Jobs | Background-job contracts, Hangfire scheduling/execution, tenant context and cron helpers. Includes example jobs. |
| Craft.Logging | Serilog configuration, a logging wrapper and request-pipeline integration. |
| Craft.Notifications | Notification preferences, channel delivery, delivery logs and optional real-time integration. Its service directly requires an EF DbContext even when persistence is disabled. |
| Craft.Auditing | Change-history models, opt-in attributes, capture/comparison/query helpers and archival. Already substantially rewritten in V11; do not port the old implementation back. |
| Craft.CryptKey | Reversible public-ID encoding using Hashids. V11 already replaces the old implementation; this is ID obfuscation, not encryption. |
| Craft.MultiTenant | Tenant resolution strategies, stores, scoped context and HTTP middleware, including cached and database-backed stores. |
| Craft.Security | Identity models, current-user access, JWT/refresh-token handling, login history and Blazor/cookie authentication integration. |
| Craft.Permissions | Permission definitions/checking, role-permission persistence, HTTP endpoints/clients and MudBlazor permission UI. |
| Craft.Controllers | Reusable entity CRUD controllers, API versioning/rate limiting and database-error translation. |
| Craft.HttpServices | Typed HTTP CRUD clients and response handling. |
| Craft.Middleware | Exception/ProblemDetails handling, request/response logging and CORS setup. |
| Craft.OpenAPI | OpenAPI/Swagger registration and configuration. |
| Craft.Hosting | Convenience registration across data, security, infrastructure, web and UI packages. This is effectively an application-stack composition package. |
| Craft.Expressions | Expression parsing/tokenization, ASTs, expression-tree construction and serialization. V11 already has this capability. |
| Craft.UiComponents | Blazor component bases, semantic HTML wrappers, interaction/JS abstractions and theming. |
| Craft.UiBuilders | Higher-level MudBlazor grids, filtering, upload controls, generic rendering helpers and UI services. |
| Craft.AppComponents | Ready-made authentication, user/role administration and audit-history screens. Also contains their server controllers, repositories and HTTP clients. |

Supporting projects:

| Project(s) | Purpose |
|---|---|
| The 28 matching `Craft.<Feature>.Tests` projects | Per-library automated tests; their names mirror all 28 source libraries above. Keep that correspondence. |
| Craft.Testing | Shared test models, fixtures and helpers; its V10 dependencies span most of the application stack. Keep common fixtures small and feature fixtures local. |
| Craft.TestHost | ASP.NET host for integration tests, currently using EF InMemory. |
| Craft.IntegrationTests | Cross-layer integration tests against the test host. |
| Craft.KeySafe | Key-generation/encryption utility under Apps; its project currently lacks executable OutputType despite containing Program.cs. Treat it as tooling, not a foundation dependency. |
| Craft.KeySafe.Tests | Tests for that utility. |

## Highest-value functionality changes

1. **Give persistence one owner.** Craft.Data should coordinate ordered save phases: detect changes and validate tenant ownership; apply soft delete and stamps; capture audit originals; save business rows; finalize generated keys and save audits/outbox rows; commit. External delivery happens after commit. Preserve V11 auditing's same-database/transaction guarantee and make auditing optional. Registration order alone is insufficient.
2. **Keep soft delete in Data.** Convert only opted-in entities, mark only intended fields modified, compose soft-delete and tenant filters, and offer explicit restore/permanent-delete operations. Specify cascade/orphan behavior, restore conflicts and bulk-operation semantics. Do not automatically rewrite every unique index or guess a provider when detection fails.
3. **Separate query intent from execution and transport.** QuerySpec should not reference controllers/HTTP clients. Keep one filter/operator vocabulary, one repository implementation path and optional serialization. Public query endpoints need an allowed field/operator set, bounded page size/includes and no client-controlled tenant-filter bypass.
4. **Unbundle application features.** Move auth/audit/permission repositories, endpoints, clients and screens into their appropriate adapters. A server should not need MudBlazor, and a browser client should not receive database/provider dependencies.
5. **Reduce foundation weight.** Move EF extensions/context contracts to Data; HTTP result mapping to web integration; document extraction/conversion to an optional Documents package; encryption out of Utilities. Keep Extensions and Utilities independent. Consolidate overlapping result/pagination types instead of adding another parallel wrapper.
6. **Remove misleading or development-only surface.** Move repository fakes, mock email providers and example jobs into tests/samples. Defer the unimplemented MySQL and cloud-secret providers until usable. Review KeySafe's reusable configured IV design before any port; do not carry custom cryptography forward by default.

Evidence anchors in the V10 snapshot: `Craft.QuerySpec.csproj` references Controllers, HttpServices and Repositories; `Craft.Permissions.csproj` references Data and UiBuilders; `Craft.Data.csproj` references Security and MultiTenant; `DbContextFeatureCollection` invokes features in list order. `SoftDeleteFeature` sets the entire entry Modified and defaults unknown provider detection to PostgreSQL. These are concrete reasons for the proposed boundaries, not claims that every affected use case is broken.

## Proposed project organization

Keep V11's existing `Source/` and `Tests/` naming; do not spend effort renaming them solely for style. Use matching groups `Core`, `Data`, `Infrastructure`, `Security`, `Web`, `UI`, plus root `docs`, `samples` and `tools`. Drop V10's numbered/spaced folder prefixes. Organize Auditing under Data when its integration is moved; folder placement need not rename its package.

| Area | Recommended boundary |
|---|---|
| Foundation | Keep Domain, Extensions, Utilities and Expressions. Review Core next: retain a small infrastructure-neutral home for genuinely shared results/pagination/context contracts, without EF/ASP.NET dependencies; do not create a second catch-all Abstractions package. |
| Persistence | Craft.Data owns EF context coordination. Keep optional Craft.Repositories and a transport-neutral Craft.QuerySpec; introduce QuerySpec.EntityFrameworkCore for execution when that separation is useful. Repository methods should not each force a commit. |
| Auditing integration | Retain Craft.Auditing's capture/model API. Move coordinator ownership to Data and connect them through an optional Craft.Data.Auditing adapter; neither base package should require the other. The current AuditedDbContext migration must be explicit, not a second competing save pipeline. |
| Database/Identity/tenancy adapters | Separate provider dependencies and Identity/tenancy integration from base Data, e.g. Data.SqlServer, Data.PostgreSql, Data.Identity and Data.MultiTenant. Start with required providers only. Avoid runtime feature switches that accidentally reuse an incompatible EF model cache. |
| Security | Separate authentication/Identity concerns from permission evaluation. Keep HTTP, EF and Blazor permission/tenancy integrations optional; core permission checks must not depend on UI. |
| Infrastructure | Keep capability packages for Cache, Emails, Files, Jobs and Notifications. Extract heavy adapters such as Redis, SMTP, Hangfire and notification persistence only where independent consumption warrants it. Use standard ILogger/options/configuration APIs rather than parallel frameworks. |
| Web | Consider combining small Controllers/Middleware integration into Craft.AspNetCore, with optional OpenAPI and query adapters. Keep HttpServices independently usable by clients. Put database-specific error classification near provider adapters, HTTP mapping at the boundary. |
| UI | Clarify names: Craft.Blazor for useful framework-neutral components and Craft.Blazor.MudBlazor for opinionated controls. Reconsider semantic wrappers that offer little beyond HTML; consolidate overlapping theme services. Split AppComponents by feature when needed, keeping backend code outside UI packages. |
| Composition | Rename/recast Hosting as an optional application bundle or starter template. Keep registration extensions with the feature being registered; the bundle must not become a dependency of lower-level libraries. |
| Tooling/testing | Place KeySafe under tools, sample jobs/apps under samples, and shared fixtures under Tests/Support. Add provider-backed integration tests; EF InMemory alone cannot validate transactions, relational filters or migrations. |

Create a project when it establishes a meaningful dependency, deployment or reuse boundary—not one project for every interface or provider stub. More adapters improve isolation but increase release/testing overhead; introduce them incrementally. Enforce inward dependencies with a small architecture check.

## Functional candidates worth adding

| Priority | Candidate | Benefit |
|---|---|---|
| First | Transactional outbox, delivery retry and consumer deduplication | Reliable jobs/emails/notifications after a business commit; at-least-once delivery with idempotent handlers, not an exactly-once promise. |
| First | Tenant isolation checks across writes, queries, caches and jobs | Makes tenancy a consistent boundary rather than just an HTTP resolution strategy. |
| First | Small end-to-end sample and package compatibility/dependency guide | Demonstrates context setup, soft delete, audit, transactions and error handling without requiring every Craft package. |
| Next | Request idempotency and optimistic-concurrency conventions | Safer retryable APIs and clearer conflict handling, aligned with existing domain concurrency contracts. |
| Next | Standard traces/metrics and correlation across save/job/delivery paths | Diagnoses failures and latency without building another logging framework. |
| Later, on demand | Cursor pagination, bounded streaming import/export, localization and feature flags | Useful business-app capabilities; add only for concrete consumers and reuse established platform facilities. |

## Suggested V11 sequence and decisions

1. Agree the dependency boundaries and review Craft.Core's responsibilities before importing it.
2. Build a minimal single-tenant Craft.Data, with soft delete, concurrency and the auditing adapter. Test generated IDs, rollback, cancellation, cascade/restore behavior and filter composition against real supported providers.
3. Add optional tenancy/Identity integration; then repositories and query specifications. Keep direct EF usage supported.
4. Add API/client integration and a working sample. Migrate infrastructure and UI by demonstrated need, not by V10 project count.
5. Introduce outbox-based external delivery before claiming reliable background side effects.

Already agreed: breaking changes are acceptable; auditing is explicit opt-in and atomic with business writes. Latest direction: soft deletion belongs in the future Data context layer, superseding the earlier repository-owned plan. Other recommendations in this review remain proposals.

Decisions to discuss before the relevant implementation:

- Should Craft remain independently consumable packages with optional full-stack starters (recommended), or primarily an opinionated application framework?
- Which production database(s) and tenant storage model must the first Data release support?
- Should authentication primarily integrate with existing identity providers, or also ship a complete local user/role/token management system?

The review does not recommend rebuilding features already present in V11 merely to match these names. Use it to guide the next boundary changes and record accepted decisions separately.
