## **Mô tả flow MCP + Gemini CLI**

### **1. Yêu cầu từ user**

* User gửi một query, ví dụ:

```
"Show me detail family with id 1a955fff-ce01-422f-8bb3-02ab14e8ec47"
```

* Input này sẽ được AiService tiếp nhận.

---

### **2. Bước Tool-Use (LLM gợi ý gọi tool)**

* **Prompt gửi đến LLM:**

  1. **System prompt**: mô tả assistant, ví dụ “You are a helpful assistant with access to specific tools.”
  2. **User prompt**: yêu cầu từ user.
  3. **Tools definition**: danh sách các tool sẵn có (`search_family`, `get_family_details`…), bao gồm `name`, `description` và `parameters` (JSON schema).
  4. **Hướng dẫn quan trọng**: LLM **không gọi API trực tiếp**, chỉ sinh JSON để gợi ý tool call.

* **Output của LLM**: một JSON object `tool_calls`, ví dụ:

```json
{
  "tool_calls": [
    {
      "id": "uuid-1234",
      "function": {
        "name": "get_family_details",
        "arguments": "{\"familyId\":\"1a955fff-ce01-422f-8bb3-02ab14e8ec47\"}"
      }
    }
  ]
}
```

* LLM **chỉ sinh JSON**, không trả text tự nhiên.

---

### **3. ToolInteractionHandler thực thi tool call**

* Nhận JSON `tool_calls` từ bước trước.
* Thực hiện API call đến backend:

  * `get_family_details(familyId)` → trả về `AiToolResult`, ví dụ:

```json
{
  "toolName": "get_family_details",
  "output": {
    "familyId": "1a955fff-ce01-422f-8bb3-02ab14e8ec47",
    "name": "Nguyen",
    "members": [
      { "id": "m1", "name": "Nguyen Van A", "dob": "1970-01-01" },
      { "id": "m2", "name": "Nguyen Van B", "dob": "1995-05-05" }
    ]
  }
}
```

* Đây là **step query dữ liệu thực tế**, tách biệt hoàn toàn với LLM.

---

### **4. Bước Chat (LLM tạo natural language response)**

* Build prompt mới cho LLM:

  1. **System prompt**: mô tả assistant.
  2. **User prompt**: yêu cầu user ban đầu.
  3. **Tool results**: kết quả từ bước trước, được serialize thành JSON và gắn role `Assistant` hoặc `Tool`.

* Output của LLM: **natural language response**:

```
Family Nguyen (ID: 1a955fff-ce01-422f-8bb3-02ab14e8ec47) has 2 members:
- Nguyen Van A, born 1970-01-01
- Nguyen Van B, born 1995-05-05
```

---

### **5. Lưu ý cho Gemini CLI**

* **Stream**: LLM nên stream từng chunk để MCP có thể gửi partial response.

* **ToolCalls schema**: phải cố định gồm 3 trường:

  * `id` (string, uuid)
  * `function.name` (string, tên tool)
  * `function.arguments` (string, JSON-escaped arguments)

* **Step phân tách rõ ràng**:

  1. Tool suggestion (JSON)
  2. Execute tool → AiToolResult
  3. Build prompt tổng hợp → LLM trả text

* **Không được** để LLM trực tiếp gọi backend.

