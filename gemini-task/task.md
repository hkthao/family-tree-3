You are an expert in Clean Architecture, DDD, and .NET development. 

Task: Refactor all **CQRS Command and Query Handlers** in my backend project to follow a **consistent Result<T> pattern** with proper **error handling** and **ErrorSource**. 

Requirements:

1. **Result<T> Implementation**
   - keep it

2. **Handler Refactoring**
   - Wrap all handler results in `Result<T>`.
   - Catch exceptions like `DbUpdateException` and general `Exception`.
   - Add **validation checks** and return `Result.Failure("message", "Validation")` when invalid input.
   - Keep the handler logic intact (do not change business logic).
   - Command handlers should still persist to DbContext / repository if used.

3. **Unit Test Generation**
   - For each handler, generate xUnit unit tests:
     - **Success scenario:** input valid, verify `IsSuccess = true` and correct `Value`.
     - **Failure scenario(s):** input invalid or simulate exception, verify `IsSuccess = false`, `Error` message, and `ErrorSource`.
   - Mock dependencies like `IApplicationDbContext` for unit tests.

4. **Coding Style**
   - Keep code clean, readable, follow C# conventions.
   - Do not change method signatures except return type to `Result<T>`.
   - Add meaningful comments where error handling is applied.

5. **Output**
   - Refactored handlers files ready to replace current ones.
   - Corresponding unit test files ready to run.
   - Include any helper files if needed (like `Result.cs`).

Goal: All handlers in the backend follow **uniform Result<T> pattern**, have proper **error handling** with **ErrorSource**, and are fully testable with unit tests.
