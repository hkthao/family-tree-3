Bạn là một senior software architect.  
Repo hiện tại tên là `family-tree-3`, đã scaffold theo cấu trúc Clean Architecture (backend ASP.NET Core, frontend Vue 3, MongoDB, tests, docs).  

👉 Nhiệm vụ: Bổ sung **unit test** vào repo.  

Yêu cầu cụ thể:  

1. **Vị trí**  
   - Tạo trong thư mục `/tests/UnitTests/`.  
   - Cấu trúc mirroring theo project backend: Application, Domain, Infrastructure.  

2. **Công nghệ**  
   - Sử dụng **xUnit** làm test framework.  
   - Sử dụng **Moq** để mock dependency.  
   - Test coverage hướng tới service layer và domain logic.  

3. **Nội dung test mẫu**  
   - `MemberServiceTests.cs`:  
     * Test tạo thành viên hợp lệ → trả về ID mới.  
     * Test tạo thành viên thiếu tên → ném exception Validation.  
   - `FamilyServiceTests.cs`:  
     * Test thêm gia đình hợp lệ.  
     * Test tìm kiếm gia đình theo tên.  
   - `RelationshipServiceTests.cs`:  
     * Test gán quan hệ cha – con.  
     * Test không cho phép tự gán quan hệ một người với chính họ.  

4. **CI/CD tích hợp**  
   - Cập nhật pipeline GitHub Actions (`.github/workflows/ci.yml`) để chạy `dotnet test` khi build backend.  

5. **Docs**  
   - Thêm hướng dẫn chạy test vào `docs/developer_guide.md`:  
     ```bash
     cd tests
     dotnet test
     ```

👉 Hãy scaffold đầy đủ file test, cập nhật pipeline và bổ sung hướng dẫn trong docs.  
