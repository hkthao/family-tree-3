# knowledge-search-service

## Mục đích service
Service này thực hiện **tìm kiếm vector có phạm vi (scoped vector search)** và được thiết kế để sử dụng như một thành phần truy xuất trong các pipeline RAG.

## API Contract

### Health Check
Kiểm tra trạng thái hoạt động của service.

`GET /health`

**Response:**
```json
{ "status": "ok" }
```

### Search API
Thực hiện tìm kiếm vector dựa trên truy vấn và các bộ lọc cho phép.

`POST /search`

**Request JSON:**
```json
{
  "family_id": "F123",
  "query": "ông tổ đời thứ 3 là ai",
  "top_k": 10,
  "allowed_visibility": ["public", "private"]
}
```

**Response JSON:**
```json
{
  "results": [
    {
      "member_id": "M001",
      "name": "Nguyễn Văn A",
      "summary": "Ông tổ đời thứ 3 của dòng họ...",
      "score": 0.87
    }
  ]
}
```

## Cách chạy local

1.  **Cài đặt dependencies:**
    ```bash
    pip install -r requirements.txt
    ```
2.  **Khởi động service:**
    ```bash
    uvicorn app.main:app --host 0.0.0.0 --port 8000
    ```
    Service sẽ chạy trên `http://localhost:8000`.

## Ví dụ request / response

Xem phần "API Contract" ở trên để biết ví dụ về request và response.

## Ghi chú quan trọng
This service performs **scoped vector search only** and is designed to be used as a retrieval component in RAG pipelines.
