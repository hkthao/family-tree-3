Bạn là một trợ lý phát triển phần mềm chuyên nghiệp. Nhiệm vụ của bạn là **viết Integration Tests bằng C# (xUnit) cho ASP.NET Core app theo DDD** dựa trên repo “Infrastructure” của app gia phả. Hãy thực hiện các yêu cầu sau:

1. **Mục tiêu Integration Test**  
   - Test luồng thực từ **Web/API → Application layer → Infrastructure → Database / External Services**.  
   - Không chỉ test unit, mà kiểm tra phối hợp giữa các layer, transaction, mapping, config.  
   - Test các luồng nghiệp vụ quan trọng: CRUD FamilyTree/Member/User, AI Chat Providers, File Storage, Auth0 login & token validation, Service layer logic.  

2. **Sequential test**  
   - Test case trước pass mới chạy test case sau.  
   - Dùng `IClassFixture` hoặc `Collection` của xUnit để đảm bảo tuần tự.

3. **Môi trường thật / sandbox**  
   - Database test container (SQL Server/Postgres) hoặc sandbox DB.  
   - External API / AI provider / File Storage dùng sandbox nếu cần.  
   - Auth0 test account cho login/token validation.  

4. **Comment tiếng Việt chi tiết**  
   - Giải thích mục tiêu test, dữ liệu đầu vào, kết quả mong đợi.  
   - Ví dụ:
     ```csharp
     // Test luồng tạo FamilyTree: tạo cây mới và validate root member
     ```

5. **Dựa trên implement thật & docs/**  
   - Không bịa selector, API, domain event, hoặc logic nghiệp vụ.  
   - Tham khảo folder `docs/` nếu cần thông tin thêm.  

6. **Setup / Teardown**  
   - Seed dữ liệu test hợp lý trước test.  
   - Cleanup database / storage sau test case nếu cần.

7. **Output yêu cầu**  
   - Một file C# xUnit Integration Test hoàn chỉnh, có thể chạy trực tiếp trong `Infrastructure.IntegrationTests/`.  
   - Test sequential các case quan trọng:  
     1. CRUD FamilyTree, Member, User  
     2. AI Chat Provider request → response → parse JSON → verify  
     3. Auth0 login + token validation  
     4. File Storage lưu / đọc / xoá file sandbox  
     5. Service layer phối hợp repository + provider + business logic  
   - Comment tiếng Việt rõ ràng từng bước.  
   - Chạy được với `dotnet test` hoặc Docker.

Hãy viết **một file Integration Test hoàn chỉnh**, bao gồm:  
- Setup database test container + DI container  
- Seed dữ liệu test cần thiết  
- Sequential test theo luồng nghiệp vụ quan trọng  
- Cleanup dữ liệu sau mỗi test case  
- Comment tiếng Việt chi tiết
