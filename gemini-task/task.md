You are an expert .NET architect specialized in DDD, CQRS, and Clean Architecture.

I have an ASP.NET Core application using MediatR and CQRS patterns for the Application layer.  
Currently, most of my command/query Handlers contain the following issues:
- They check if the user is logged in using ICurrentUser.Id == null.
- They use hard-coded error messages like "Access denied" or "User not logged in".
- They manually set CreatedBy and UpdatedBy fields for every entity before saving.
- Authorization checks (business-level permissions) are mixed with authentication checks.
- Unit tests become complicated due to repeated login and error logic.

Your task is to **refactor all Handlers in the Application layer** to make them clean, consistent, and testable.

### Refactor Goals

1. **Remove all login checks from Handlers.**  
   - Authentication should be handled globally by middleware or MediatR pipeline.
   - If user is not logged in, block the request before reaching the handler.

2. **Introduce a global AuthorizationBehavior<TRequest, TResponse> (MediatR Pipeline)**  
   - Automatically validates whether a user is authenticated using ICurrentUser.
   - Throws UnauthorizedAccessException if not.

3. **Use a centralized ErrorMessages static class**  
   - Example:
     ```csharp
     public static class ErrorMessages
     {
         public const string AccessDenied = "Access denied. You do not have permission to perform this action.";
         public const string NotFound = "{0} not found.";
         public const string Unauthorized = "User is not authenticated.";
     }
     ```
   - Replace all hard-coded strings in handlers with references to ErrorMessages.

5. **Simplify Handlers:**
   - Keep only business rules (e.g., CanManageFamily, CanEditEvent).
   - Keep domain events intact.
   - Return `Result<T>` objects for all responses (no raw exceptions).

6. **Ensure naming conventions follow DDD standards:**
   - Commands: `CreateEventCommand`, `UpdateEventCommand`
   - Queries: `GetFamilyMembersQuery`
   - Handlers: `[CommandName]Handler`, `[QueryName]Handler`
   - Use folders `/Commands/`, `/Queries/`, `/Validators/` within each feature.

7. **Keep Handlers short and readable:**
   - Max 50–70 lines per handler.
   - Extract long domain logic to domain services if needed.

8. **Demonstrate refactor with one example:**
   - Rewrite `UpdateEventCommandHandler` to:
     - Remove login check.
     - Use `_authorizationService.CanManageFamily(...)`
     - Use `ErrorMessages` constants.
     - Rely on DbContext audit for CreatedBy/UpdatedBy.
     - Fire domain event `EventUpdatedEvent`.

### Deliverables
- Refactored example handler (UpdateEventCommandHandler).
- Example of AuthorizationBehavior<TRequest, TResponse>.
- Example of DbContext audit configuration.
- Example of ErrorMessages static class.
- Consistent folder and file naming convention for Application layer.

Output should be clean, production-ready C# code, formatted using typical .NET conventions.
