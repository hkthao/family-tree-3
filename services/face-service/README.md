# Face Service - Dịch vụ Xử lý Khuôn mặt

Dịch vụ này cung cấp các chức năng liên quan đến phát hiện, nhúng (embedding) và quản lý dữ liệu khuôn mặt, tích hợp với hệ thống lưu trữ vector Qdrant và hàng đợi tin nhắn RabbitMQ.

## 1. Tổng quan Dự án

Face Service là một phần quan trọng trong dự án "Dòng Họ Việt", chịu trách nhiệm:
*   Phát hiện khuôn mặt trong hình ảnh được cung cấp.
*   Tạo ra các vector nhúng (embeddings) cho khuôn mặt để nhận dạng và so sánh.
*   Quản lý dữ liệu khuôn mặt (thêm, xóa, tìm kiếm) trong cơ sở dữ liệu vector Qdrant.
*   Xử lý các sự kiện liên quan đến khuôn mặt thành viên thông qua hàng đợi tin nhắn.

## 2. Kiến trúc (Clean Architecture)

Dịch vụ được tổ chức theo kiến trúc sạch (Clean Architecture) để tăng cường khả năng mở rộng, dễ kiểm thử và tách biệt các mối quan tâm. Cấu trúc bao gồm các lớp chính:

*   **Domain (Lớp Nghiệp vụ cốt lõi):** Chứa các thực thể (entities) và định nghĩa giao diện (interfaces) cho các dịch vụ (ví dụ: `IFaceDetector`, `IFaceEmbedding`, `IFaceRepository`). Đây là lớp độc lập với bất kỳ framework hoặc cơ sở dữ liệu cụ thể nào.
*   **Application (Lớp Ứng dụng):** Chứa các quy tắc nghiệp vụ cụ thể của ứng dụng (use cases) và các dịch vụ ứng dụng (`FaceManager`). Lớp này điều phối các hoạt động giữa lớp Domain và lớp Infrastructure.
*   **Infrastructure (Lớp Hạ tầng):** Chứa các triển khai cụ thể của các giao diện được định nghĩa trong lớp Domain. Bao gồm các dịch vụ phát hiện khuôn mặt (Dlib), nhúng khuôn mặt (FaceNet), tương tác với Qdrant và xử lý tin nhắn RabbitMQ.
*   **Presentation (Lớp Trình bày):** Lớp giao diện người dùng/API. Trong trường hợp này là API FastAPI, chịu trách nhiệm xử lý các yêu cầu HTTP, chuyển chúng đến lớp Application và trả về phản hồi.

## 3. Công nghệ sử dụng (Tech Stack)

*   **Framework:** FastAPI
*   **Thư viện xử lý ảnh:** Dlib, OpenCV (qua `cv2`), PIL (Pillow)
*   **Vector Database:** Qdrant Client
*   **Message Queue:** `aio-pika` (RabbitMQ Client)
*   **Ngôn ngữ:** Python 3.10+
*   **Môi trường:** Docker, Docker Compose

## 4. Bắt đầu nhanh (Getting Started)

### Yêu cầu

*   Docker & Docker Compose (để chạy dịch vụ và Qdrant)
*   Python 3.10+ (nếu chạy cục bộ bên ngoài Docker)

### Cài đặt và Chạy

1.  **Clone kho lưu trữ chính của dự án:**
    ```bash
    git clone https://github.com/your-username/family-tree-3.git
    cd family-tree-3/services/face-service
    ```

2.  **Chạy dịch vụ bằng Docker Compose:**
    Dịch vụ Face Service được thiết kế để chạy trong môi trường Docker Compose cùng với các dịch vụ khác của dự án. Từ thư mục gốc của dự án `family-tree-3`, chạy lệnh sau:
    ```bash
    docker-compose -f infra/docker-compose.yml up --build -d face-service
    ```
    Lệnh này sẽ build và khởi chạy chỉ `face-service` cùng với các dependency của nó (ví dụ: Qdrant, RabbitMQ).

3.  **Truy cập API:**
    Sau khi dịch vụ khởi động, bạn có thể truy cập tài liệu API Swagger tại:
    [http://localhost:8080/face-service/docs](http://localhost:8080/face-service/docs)

### Chạy thử nghiệm (Running Tests)

Để chạy tất cả các bài kiểm thử đơn vị và tích hợp cho Face Service, hãy làm theo các bước sau:

1.  **Đảm bảo bạn đang ở thư mục `services/face-service`:**
    ```bash
    cd family-tree-3/services/face-service
    ```

2.  **Kích hoạt môi trường ảo (nếu có) và cài đặt dependencies:**
    ```bash
    python -m venv .venv
    source .venv/bin/activate
    pip install -r requirements.txt
    pip install pytest pytest-asyncio
    ```

3.  **Chạy Pytest:**
    ```bash
    pytest
    ```
    Tất cả các bài kiểm thử phải vượt qua.

## 5. Cấu trúc Dự án

```
.
├── Dockerfile                  # Định nghĩa Docker image cho dịch vụ
├── requirements.txt            # Danh sách các thư viện Python cần thiết
├── src/                        # Thư mục chứa mã nguồn chính
│   ├── application/            # Lớp ứng dụng
│   │   └── services/           # Chứa FaceManager (logic nghiệp vụ chính)
│   │       └── face_manager.py
│   ├── domain/                 # Lớp nghiệp vụ cốt lõi
│   │   ├── entities/           # Các Pydantic models (data structures)
│   │   │   └── models.py
│   │   └── interfaces/         # Các Abstract Base Classes (ABCs) cho services
│   │       ├── face_detector.py
│   │       ├── face_embedding.py
│   │       └── face_repository.py
│   ├── infrastructure/         # Lớp hạ tầng (triển khai cụ thể)
│   │   ├── detectors/          # Triển khai FaceDetector (ví dụ: DlibFaceDetector)
│   │   │   └── dlib_detector.py
│   │   ├── embeddings/         # Triển khai FaceEmbedding (ví dụ: FaceNetEmbeddingService)
│   │   │   └── facenet_embedding.py
│   │   ├── message_bus/        # Triển khai MessageConsumer
│   │   │   └── consumer_impl.py
│   │   └── persistence/        # Triển khai FaceRepository (ví dụ: QdrantFaceRepository)
│   │       └── qdrant_client.py
│   └── presentation/           # Lớp trình bày (FastAPI)
│       ├── api/                # Các định nghĩa API routes
│       │   └── v1/
│       │       └── endpoints/
│       │           └── face_endpoints.py
│       ├── dependencies.py     # Cấu hình Dependency Injection
│       └── main.py             # Điểm vào ứng dụng FastAPI
└── tests/                      # Thư mục chứa các bài kiểm thử
    ├── test_face_detector.py
    ├── test_face_embedding.py
    ├── test_face_service.py
    ├── test_main_endpoints.py
    ├── test_message_consumer.py
    └── test_qdrant_service.py
```

## 6. API Endpoints

Dịch vụ này cung cấp các endpoint API sau:

*   `POST /detect`
    *   Phát hiện khuôn mặt trong một hình ảnh và trả về thông tin bounding box, độ tin cậy, và embedding khuôn mặt.
*   `POST /faces`
    *   Thêm một khuôn mặt mới vào hệ thống cùng với metadata.
*   `POST /faces/vector`
    *   Thêm một khuôn mặt mới bằng cách cung cấp trực tiếp vector embedding và metadata.
*   `POST /faces/search`
    *   Tìm kiếm các khuôn mặt tương tự trong cơ sở dữ liệu dựa trên một hình ảnh truy vấn.
*   `POST /faces/search_by_vector`
    *   Tìm kiếm các khuôn mặt tương tự trong cơ sở dữ liệu dựa trên một vector embedding truy vấn.
*   `GET /faces/family/{family_id}`
    *   Truy xuất tất cả các khuôn mặt thuộc về một `family_id` cụ thể.
*   `DELETE /faces/{face_id}`
    *   Xóa một khuôn mặt cụ thể khỏi hệ thống bằng `face_id`.
*   `DELETE /faces/family/{family_id}`
    *   Xóa tất cả các khuôn mặt thuộc về một `family_id` cụ thể.
