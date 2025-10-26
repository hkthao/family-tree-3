You are a senior .NET test architect with experience in clean testing, CQRS, and MediatR-based applications.

The project has just completed a major refactor of all Command and Query Handlers:
- Removed all login checks from handlers (authentication is handled by pipeline).
- Added AuthorizationBehavior<TRequest, TResponse> for authentication checks.
- Centralized error messages in ErrorMessages static class.
- Auto-populated audit fields (CreatedBy, UpdatedBy) inside DbContext.
- Simplified handler logic to focus only on business rules and domain events.

Now I need you to **rebuild all existing unit tests** for the Application layer so they align with the new architecture.

### ✅ Goals

1. **Update all tests** (e.g. `UpdateEventCommandHandlerTests`, `CreateFamilyCommandHandlerTests`, etc.)
   - Remove login-related test cases (no need to test "user not logged in").
   - Keep or add test cases for:
     - Success path (happy flow)
     - Authorization failure (`CanManageFamily` returns false)
     - Entity not found
     - Domain event raised (if applicable)
     - Database save verified

2. **Use xUnit + Moq** for mocking.
   - Mock dependencies like `_context`, `_authorizationService`, `_currentUser`.
   - Test result should be of type `Result<T>` and assert Success / Failure accordingly.

3. **Add clear, simple Vietnamese comments in every test method and setup.**
   - Dễ hiểu cho **tester**, **junior dev**, **reviewer**.
   - Ví dụ:
     ```csharp
     // Mô tả: Kiểm tra khi người dùng có quyền quản lý gia đình thì cập nhật sự kiện thành công.
     // Kết quả mong đợi: Trả về Success = true và sự kiện được cập nhật trong DB.
     ```

4. **Add summary comment block at the top of each test file:**
   ```csharp
   /*
    * Tên file: UpdateEventCommandHandlerTests.cs
    * Mục đích: Kiểm thử logic cập nhật sự kiện sau khi refactor (loại bỏ check login).
    * Đối tượng kiểm thử: UpdateEventCommandHandler.
    * Phạm vi: Unit test mức Application, sử dụng Moq và in-memory DbContext.
    * Người đọc: Tester, Dev, Junior Dev đều có thể hiểu dễ dàng.
    */
Follow consistent structure for all tests:

Arrange: Chuẩn bị dữ liệu và mock.

Act: Gọi Handler.

Assert: Kiểm tra kết quả mong đợi.

Example test case to show in output:

UpdateEventCommandHandlerTests

Test success scenario.

Test “event not found”.

Test “unauthorized user”.
Each with comments as described.

Output format:

Fully working C# code with Vietnamese comments and /// <summary> for test class.

Deliverables

Updated test files for all command and query handlers.

Each test contains Vietnamese comments explaining purpose, steps, and expected result.

Summary block at top of each file.

Ready-to-run xUnit tests with Moq.