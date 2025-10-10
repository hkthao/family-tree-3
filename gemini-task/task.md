# Prompt: Refactor AI Settings Architecture

## Context
Hiện tại hệ thống có nhiều lớp cấu hình tương tự nhau:
- `EmbeddingSettings`
- `ChatAISettings`
- `AIContentGeneratorSettings`

Mỗi lớp đều có cấu trúc gồm `Provider` (OpenAI, Gemini, v.v.) và các nhóm cấu hình con (ví dụ: `OpenAISettings`, `GeminiSettings`).  
Tuy nhiên, cách tổ chức hiện tại đang lặp lại nhiều, khó mở rộng và rối khi thêm provider mới.

## Task
Refactor toàn bộ các lớp *Settings* liên quan đến AI sao cho:
1. **Đơn giản & tái sử dụng cao** — tránh lặp lại giữa các Settings khác nhau.
2. **Mở rộng linh hoạt** — dễ thêm provider mới (như Claude, Mistral...).
3. **Hỗ trợ IConfiguration binding** (từ `appsettings.json`) tự động.
4. **Giữ nguyên backward compatibility** cho các module đang dùng hiện tại.

## Implementation Hints
- Tạo interface tổng quát `IAIProviderSettings` (ApiKey, Model...).
- Dùng generic class:  
  ```csharp
  public class AIProviderConfig<TProviderSettings> where TProviderSettings : IAIProviderSettings
````

* Cho phép kế thừa trong các setting cụ thể:

  ```csharp
  public class EmbeddingSettings : AIProviderConfig<IOpenAISettings> { }
  ```
* Đảm bảo `SectionName` có thể được cấu hình riêng cho từng module.
* Tích hợp hợp lý vào `IOptions<EmbeddingSettings>`.

## Deliverables

* Code refactor hoàn chỉnh.
* Mẫu `appsettings.json` mới tương thích.
