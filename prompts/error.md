Bạn là một senior .NET developer. Tôi đang build project ASP.NET Core với MongoDB và NSwag để generate Swagger/OpenAPI. Trên local build chạy bình thường nhưng trên GitHub Actions thì NSwag fail vì lỗi `ArgumentNullException: Value cannot be null. (Parameter 'connectionString')`.  

Yêu cầu:
1. Viết hướng dẫn chi tiết để NSwag chạy trên CI mà không cần database thật.
2. Bao gồm cách:
   - Thêm biến môi trường fake cho connection string.
   - Cập nhật Program/DI để skip init MongoDB khi chạy CI.
   - Cấu hình NSwag.MSBuild hoặc GitHub Actions.
3. Đưa ví dụ YAML GitHub Actions, code C# Program/DI đầy đủ.
4. Giải thích lý do tại sao trên local không lỗi nhưng trên CI thì lỗi.
5. Tóm tắt các cách fix khác nếu có.

Trả lời theo dạng chi tiết step-by-step, dễ copy-paste, không bỏ sót bước nào.
