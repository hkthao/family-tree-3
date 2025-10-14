> Xây dựng backend ASP.NET Core theo mô hình **Domain-Driven Design (DDD)** cho hệ thống nhập liệu gia phả bằng ngôn ngữ tự nhiên.
>
> Hệ thống gồm các layer:
>
> * **Domain:** chứa entity `Person`, `Relationship` và logic nghiệp vụ.
> * **Application:** định nghĩa command/handler xử lý các tác vụ như phân tích câu nhập và lưu dữ liệu.
> * **Infrastructure:** triển khai các dịch vụ giao tiếp bên ngoài, đặc biệt là **`_chatProviderFactory`** dùng để chọn provider NLP.
> * **Presentation:** cung cấp API cho frontend gửi prompt và xác nhận lưu.
>
> `_chatProviderFactory` hỗ trợ nhiều provider khác nhau như `Ollama`, `OpenAI`, và `Gemini`, tất cả tuân theo interface chung `IChatProvider`.
> Mỗi provider có nhiệm vụ phân tích prompt ngôn ngữ tự nhiên và trả về dữ liệu có cấu trúc (tên, năm sinh, quan hệ, đối tượng).
>
> Backend sử dụng factory này để gọi LLM tương ứng, xử lý kết quả, ánh xạ sang domain entity, và lưu dữ liệu theo chuẩn DDD.
> Mục tiêu là **tách biệt hoàn toàn logic nghiệp vụ khỏi hạ tầng AI**, cho phép thay đổi provider linh hoạt chỉ bằng cấu hình.
