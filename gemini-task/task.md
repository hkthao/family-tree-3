

You are a senior .NET test architect with experience in clean testing, CQRS, and MediatR-based applications.

- Doc lai cac logic trong Application loai bo cac test khong phu hop
- implement test phan anh dung logic hien tai cua Application 
- Khong dc sua code cua Application
- comment lai thanh dang XML summary BAT BUOC
    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi một FamilyId không hợp lệ (không tồn tại) được cung cấp trong command.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và thiết lập _mockUser.Id. Thiết lập _mockAuthorizationService để CanManageFamily trả về false cho FamilyId không tồn tại. Tạo một UpdateEventCommand với một FamilyId không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại. Kiểm tra thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể cập nhật sự kiện cho một gia đình không tồn tại,
    /// ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldIncludeRolesInUserProfileDto(){}
- moi lan chi implement cho 1 test khi test pass het case thi moi qua test khac
- Moi test implement it nhat 3 - 4 case quan trong 
- Loai bo cac comment, inject du thua
- Sau khi Test pass het case thi phai commit & push changes
- Khong dung cac hard text de Assert, dung cac constants nhu: ErrorMessages, ErrorSources da dc dinh nghia trong Application
