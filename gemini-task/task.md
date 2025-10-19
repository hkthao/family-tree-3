You are working on an ASP.NET Core backend following Domain-Driven Design (DDD) with a CQRS pattern.  
The application is already deployed in production, so **you must not break or refactor existing structure**.  
The system currently stores configuration in `appsettings.json`.

Your task is to implement a **System Configuration module** that allows admins and users to manage dynamic configuration values at runtime.

---

### Technical context
- The application uses **EF Core** directly (no repository layer).
- CQRS is implemented using distinct **Commands** and **Queries** with handlers.
- Configuration values are currently injected through `IOptions<AppSettings>`.

---

### Requirements

1. **Preserve current architecture.**
   - Do not modify existing handlers, services, or the `AppSettings` structure.
   - Only extend the system by adding new entities, DbSets, and CQRS handlers.

2. **Introduce three configuration layers:**
   - **Fixed Config:** stays in `appsettings.json`, not editable at runtime.
   - **System Config:** editable by admin users; stored in the database.

3. **Implementation details:**
   - Create EF Core entities for `SystemConfig` and `UserConfig`.
   - Add them to the existing `DbContext`.
   - Create CQRS handlers:
     - Commands for setting/updating config values.
     - Queries for fetching config values (single and list).
   - Implement a **ConfigurationProvider** service that reads values in the following priority:
     `SystemConfig → AppSettings`.
   - Inject this provider where dynamic configuration is needed.

4. **Integration:**
   - Register the provider and handlers via DI.
   - Ensure backward compatibility with all existing `IOptions<AppSettings>` consumers.
   - Keep all naming and folder conventions consistent with current CQRS structure.
   - No breaking changes to startup or runtime behavior.

5. **Optional (if feasible):**
   - Implement minimal Admin API endpoints for reading/updating system configuration.
   - Support caching (e.g., MemoryCache) for frequently accessed settings.

---

### Goal
Implement a production-safe, database-backed system configuration layer compatible with EF Core and CQRS, extending the existing DDD structure **without refactoring or breaking current code paths**.
