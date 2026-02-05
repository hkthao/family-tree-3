# Cấu trúc Dự án (Project Structure)

Tài liệu này cung cấp cái nhìn tổng quan chi tiết về cấu trúc thư mục của dự án "Cây Gia Phả", được tổ chức theo mô hình monorepo. Cấu trúc này nhằm mục đích tạo điều kiện thuận lợi cho việc quản lý mã nguồn, phát triển và bảo trì các ứng dụng và dịch vụ.

## 1. Tổng quan về Monorepo

Dự án được quản lý như một kho lưu trữ (repository) đơn lẻ chứa mã nguồn của nhiều dự án con khác nhau (ứng dụng, thư viện, dịch vụ). Cách tiếp cận monorepo giúp:
*   **Tái sử dụng mã:** Dễ dàng chia sẻ các thành phần, thư viện, hoặc kiểu dữ liệu giữa các dự án con.
*   **Quản lý phụ thuộc:** Tập trung hóa việc quản lý các phiên bản thư viện và công cụ.
*   **Đồng bộ hóa:** Dễ dàng thực hiện các thay đổi trên toàn hệ thống một cách đồng bộ.
*   **Phát triển liền mạch:** Tăng cường sự hợp tác giữa các nhóm phát triển.

## 2. Cấu trúc Thư mục Gốc

Dưới đây là cấu trúc thư mục cấp cao nhất của dự án:

```
family-tree-3/
├── .github/              # Cấu hình GitHub Actions CI/CD
├── apps/                 # Các ứng dụng độc lập
├── docs/                 # Toàn bộ tài liệu dự án
├── infra/                # Cấu hình hạ tầng và triển khai Docker

├── services/             # Các microservice phụ trợ
├── tests/                # Các bài kiểm thử tổng thể
├── .gitignore            # Các tệp/thư mục bị Git bỏ qua
├── CODE_OF_CONDUCT.md    # Quy tắc ứng xử
├── GEMINI.md             # Ghi chú nội bộ của Gemini CLI
├── LICENSE               # Giấy phép của dự án
├── README.md             # Tổng quan dự án và hướng dẫn bắt đầu nhanh
├── package.json          # Cấu hình dự án (monorepo level, nếu có)
└── ...
```

## 3. Thư mục `apps/`

Chứa các ứng dụng chính có thể chạy và triển khai độc lập:

*   **`apps/admin/`**:
    *   **Mô tả:** Ứng dụng giao diện người dùng (frontend) dành cho quản trị viên và người dùng cuối.
    *   **Công nghệ:** Vue.js 3, TypeScript, Vuetify 3.
    *   **Cấu trúc chi tiết (`apps/admin/src/`):**
        *   `assets/`: Các tệp tài nguyên tĩnh (hình ảnh, fonts, v.v.).
        *   `components/`: Các thành phần UI có thể tái sử dụng.
        *   `composables/`: Các hàm có thể tái sử dụng (Composition API).
        *   `constants/`: Các hằng số của ứng dụng.
        *   `data/`: Dữ liệu tĩnh hoặc mock data.
        *   `layouts/`: Bố cục trang chung.
        *   `locales/`: Các tệp bản địa hóa (i18n).
        *   `plugins/`: Các plugin Vue.js.
        *   `router/`: Cấu hình Vue Router.
        *   `services/`: Các dịch vụ API tương tác với backend.
        *   `stores/`: Các module Pinia để quản lý trạng thái.
        *   `styles/`: Các tệp CSS/SCSS tùy chỉnh.
        *   `types/`: Các định nghĩa kiểu TypeScript.
        *   `utils/`: Các hàm tiện ích chung.
        *   `validations/`: Các quy tắc validation.
        *   `views/`: Các trang hoặc màn hình chính của ứng dụng.
        *   `App.vue`: Component gốc của ứng dụng.
        *   `main.ts`: Điểm khởi đầu của ứng dụng Vue.js.
*   **`apps/backend/`**:
    *   **Mô tả:** Dịch vụ API backend cung cấp các chức năng cốt lõi của hệ thống.
    *   **Công nghệ:** ASP.NET 8, C#, Entity Framework Core, MySQL.
    *   **Cấu trúc chi tiết (`apps/backend/src/` - theo Clean Architecture):**
        *   `Application/`: Chứa logic nghiệp vụ cấp cao, định nghĩa các lệnh (Commands), truy vấn (Queries), bộ xử lý (Handlers), interfaces cho các dịch vụ ứng dụng.
        *   `CompositionRoot/`: Cấu hình Dependency Injection và khởi tạo các dịch vụ.
        *   `Domain/`: Chứa các thực thể (Entities), đối tượng giá trị (Value Objects), domain events, các quy tắc nghiệp vụ cốt lõi.
        *   `Infrastructure/`: Chứa việc triển khai các interfaces từ Application, tích hợp với cơ sở dữ liệu (EF Core), hệ thống tệp, dịch vụ bên ngoài.
        *   `Web/`: Chứa các API controllers, DTOs, cấu hình web, điểm khởi đầu của ứng dụng (Startup/Program.cs).
*   **`apps/tests/`**:
    *   **Mô tả:** Chứa các bài kiểm thử end-to-end (E2E) hoặc kiểm thử tích hợp lớn cho toàn bộ hệ thống, hoặc các tiện ích kiểm thử dùng chung cho nhiều ứng dụng.

## 4. Thư mục `services/`

Chứa các microservice độc lập, mỗi dịch vụ đảm nhiệm một chức năng cụ thể, hỗ trợ các ứng dụng chính:

*   **`services/face-service/`**:
    *   **Mô tả:** Dịch vụ xử lý khuôn mặt bằng Python.
    *   **Cấu trúc chi tiết (`services/face-service/src/`):**
        *   `application/`: Chứa logic nghiệp vụ cấp cao.
        *   `domain/`: Chứa các định nghĩa và quy tắc nghiệp vụ cốt lõi.
        *   `infrastructure/`: Chứa việc triển khai các phụ thuộc bên ngoài (ví dụ: API của bên thứ ba, lưu trữ).
        *   `presentation/`: Chứa các giao diện hoặc điểm cuối API để tương tác với dịch vụ.
*   **`services/graphviz-pdf-converter/`**: Dịch vụ chuyển đổi Graphviz DOT sang PDF bằng Python.
*   **`services/knowledge-search-service/`**: Dịch vụ tìm kiếm kiến thức bằng Python.
*   **`services/llm-gateway-service/`**: Dịch vụ cổng truy cập các mô hình ngôn ngữ lớn (LLM) bằng Python.
*   **`services/notification-service/`**: Dịch vụ quản lý và gửi các loại thông báo khác nhau bằng Node.js.
*   **`services/storage-service/`**: Dịch vụ quản lý lưu trữ tệp tin bằng Node.js.



## 5. Thư mục `infra/`

Chứa các tệp cấu hình và tập lệnh cần thiết để xây dựng, triển khai và chạy hệ thống:

*   **`docker-compose.yml`**: Tệp cấu hình chính cho Docker Compose, định nghĩa các dịch vụ, mạng và volume cho môi trường phát triển cục bộ.
*   **`docker-compose.prod.yml`**: Tệp cấu hình Docker Compose dành cho môi trường sản phẩm, với các thiết lập tối ưu hóa cho môi trường triển khai.
*   **`nginx/`**:
    *   **Mô tả:** Chứa cấu hình cho máy chủ web Nginx, thường được sử dụng làm reverse proxy cho frontend và backend.
    *   **Cấu trúc chi tiết:** `default.conf` (tệp cấu hình Nginx chính).
*   **`promtail-config.yaml`**: Cấu hình cho Promtail, một tác nhân thu thập nhật ký để gửi nhật ký đến Loki.
*   **`app/`**:
    *   **Mô tả:** Chứa các scripts hoặc cấu hình chung liên quan đến việc khởi tạo hoặc quản lý ứng dụng trong môi trường Docker.
    *   **Cấu trúc chi tiết:** `models/` (có thể chứa các mô hình dữ liệu hoặc cấu hình liên quan đến ứng dụng).

## 6. Thư mục `docs/`

Chứa toàn bộ tài liệu dự án, được phân loại thành các thư mục con để dễ quản lý:

*   **`docs/engineering/`**: Tài liệu kỹ thuật dành cho nhà phát triển (kiến trúc, quy ước mã hóa, hướng dẫn phát triển, v.v.).
*   **`docs/project/`**: Tài liệu quản lý dự án (backlog, kế hoạch, thông tin nhóm, v.v.).

## 7. Thư mục `.github/`

Chứa các cấu hình cho GitHub Actions, định nghĩa các workflow CI/CD để tự động hóa quá trình build, kiểm thử và triển khai:

*   **`workflows/`**: Chứa các tệp YAML định nghĩa các pipeline CI/CD.

---
**Lưu ý:** Cấu trúc này có thể được cập nhật khi dự án phát triển hoặc khi có các yêu cầu mới.
