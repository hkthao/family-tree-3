Bạn là chuyên gia lập trình C# và .NET, hiểu chuẩn XML Documentation của Microsoft, đồng thời viết comment bằng tiếng Việt chuẩn, rõ ràng.

Tôi có một repository ASP.NET Core chưa có comment.  
Nhiệm vụ của bạn là tự động thêm comment **bằng tiếng Việt** cho tất cả code, bao gồm:

1. **Class, struct, enum**
2. **Properties** (kể cả class được trả về)
3. **Method, function, controller action, service**
4. **Tham số đầu vào** (`<param>`)
5. **Giá trị trả về** (`<returns>`)
6. **Exceptions nếu có** (`<exception>`)
7. **HTTP verb** cho controller action (ví dụ GET, POST…)

### Yêu cầu chi tiết:
- Sử dụng tiếng Việt chuẩn, dễ hiểu, mô tả rõ ràng mục đích, chức năng.  
- Không thay đổi logic hoặc hành vi code — chỉ thêm comment.  
- Nếu method trả về một object, hãy thêm mô tả **từng property của object**.  
- Đảm bảo comment cho mọi **public** và **internal** member.  
- Trả về **file source code đã comment** hoặc patch cập nhật.

### Ví dụ comment:

#### Class + Property
```csharp
/// <summary>
/// Thông tin khách hàng
/// </summary>
public class Customer
{
    /// <summary>
    /// Tên đầy đủ của khách hàng
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Tuổi của khách hàng
    /// </summary>
    public int Age { get; set; }
}

/// <summary>
/// Lấy danh sách khách hàng theo tuổi
/// </summary>
/// <param name="minAge">Tuổi tối thiểu</param>
/// <param name="maxAge">Tuổi tối đa</param>
/// <returns>Danh sách khách hàng thỏa điều kiện</returns>
public List<Customer> GetCustomersByAge(int minAge, int maxAge)

/// <summary>
/// Xử lý GET request để lấy danh sách khách hàng theo tuổi
/// </summary>
/// <param name="minAge">Tuổi tối thiểu</param>
/// <param name="maxAge">Tuổi tối đa</param>
/// <returns>Danh sách khách hàng thỏa điều kiện</returns>
[HttpGet("customers/by-age")]
public List<Customer> GetCustomersByAge(int minAge, int maxAge)
