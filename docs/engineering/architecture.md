# Kiến trúc Hệ thống

Tài liệu này cung cấp cái nhìn tổng quan về kiến trúc của dự án "Cây Gia Phả", được thiết kế theo cấu trúc monorepo và tuân thủ các nguyên tắc của Clean Architecture. Mục tiêu là xây dựng một hệ thống mạnh mẽ, dễ bảo trì và mở rộng.

## 1. Cấu trúc Monorepo

Dự án được tổ chức như một monorepo, giúp quản lý tập trung mã nguồn của nhiều ứng dụng và dịch vụ liên quan. Cấu trúc này tạo điều kiện thuận lợi cho việc chia sẻ mã, đồng bộ hóa phiên bản và phát triển liền mạch.

Các thư mục chính bao gồm:

*   `apps/`: Chứa các ứng dụng độc lập, có thể triển khai riêng biệt.
*   `services/`: Chứa các microservice chuyên biệt hỗ trợ các ứng dụng chính.
*   `infra/`: Chứa cấu hình hạ tầng và các tệp triển khai.

## 2. Các Ứng dụng (apps/)

Thư mục `apps/` chứa các ứng dụng chính của hệ thống:

### 2.1. `apps/admin/` - Giao diện Quản trị (Frontend)

*   **Mô tả:** Ứng dụng giao diện người dùng (frontend) dành cho quản trị viên và người dùng cuối để tương tác với hệ thống. Đây là nơi người dùng thực hiện các thao tác quản lý dữ liệu gia phả, cài đặt và xem báo cáo.
*   **Công nghệ:** Vue.js 3, TypeScript, Vuetify 3, Pinia, Vue Router, Vite.
*   **Chức năng chính:** Quản lý thành viên, dòng họ, quan hệ, trực quan hóa cây gia phả, tìm kiếm, v.v.

### 2.2. `apps/backend/` - API Backend

*   **Mô tả:** Ứng dụng backend cung cấp API RESTful cho các ứng dụng frontend và các dịch vụ khác. Nó chịu trách nhiệm xử lý logic nghiệp vụ, quản lý dữ liệu và tích hợp với các dịch vụ bên ngoài.
*   **Kiến trúc:** Clean Architecture (Domain, Application, Infrastructure, Web), CQRS.
*   **Công nghệ:** ASP.NET 8, C#, Entity Framework Core, MySQL, JWT Authentication.
*   **Chức năng chính:** Xác thực/ủy quyền, quản lý dữ liệu gia phả, xử lý yêu cầu từ frontend, tích hợp với cơ sở dữ liệu.

### 2.3. `apps/tests/` - Kiểm thử Toàn cục (Global Tests)

*   **Mô tả:** Chứa các bài kiểm thử end-to-end (E2E) hoặc kiểm thử tích hợp lớn (large-scale integration tests) cho toàn bộ hệ thống, hoặc các tiện ích kiểm thử dùng chung cho nhiều ứng dụng. (Cần được làm rõ thêm nếu có nội dung cụ thể).

## 3. Các Dịch vụ (services/)

Thư mục `services/` chứa các microservice độc lập, mỗi dịch vụ đảm nhiệm một chức năng cụ thể:

### 3.1. `services/face-service/` - Dịch vụ Xử lý Khuôn mặt

*   **Mô tả:** Dịch vụ chuyên biệt để xử lý các tác vụ liên quan đến khuôn mặt, chẳng hạn như nhận diện, so sánh, hoặc trích xuất đặc trưng khuôn mặt từ hình ảnh.
*   **Công nghệ:** Python.



### 3.2. `services/graphviz-pdf-converter/` - Dịch vụ Chuyển đổi Graphviz sang PDF

*   **Mô tả:** Dịch vụ dùng để chuyển đổi định dạng Graphviz DOT sang tài liệu PDF, phục vụ cho việc tạo ra các biểu đồ hoặc báo cáo phức tạp.
*   **Công nghệ:** Python, Graphviz.

### 3.3. `services/knowledge-search-service/` - Dịch vụ Tìm kiếm Kiến thức

*   **Mô tả:** Dịch vụ chịu trách nhiệm về chức năng tìm kiếm thông tin, kiến thức trong hệ thống, có thể bao gồm tìm kiếm toàn văn hoặc tìm kiếm ngữ nghĩa.
*   **Công nghệ:** Python.

### 3.4. `services/llm-gateway-service/` - Dịch vụ Cổng LLM

*   **Mô tả:** Cổng truy cập và quản lý các mô hình ngôn ngữ lớn (LLM - Large Language Models), cho phép các ứng dụng khác trong hệ thống tận dụng khả năng của AI để tạo nội dung, tóm tắt, hoặc phân tích ngôn ngữ.
*   **Công nghệ:** Python.

### 3.5. `services/notification-service/` - Dịch vụ Thông báo

*   **Mô tả:** Dịch vụ quản lý và gửi các loại thông báo khác nhau (ví dụ: email, SMS, thông báo trong ứng dụng) đến người dùng.
*   **Công nghệ:** Node.js.

### 3.6. `services/storage-service/` - Dịch vụ Lưu trữ

*   **Mô tả:** Dịch vụ chuyên trách việc quản lý lưu trữ tệp tin (ví dụ: ảnh, tài liệu) cho hệ thống, cung cấp các API để tải lên, tải xuống và quản lý các đối tượng lưu trữ.
*   **Công nghệ:** Node.js.

## 4. Hạ tầng (infra/)

Thư mục `infra/` chứa các tệp cấu hình và tập lệnh cần thiết để xây dựng, triển khai và chạy hệ thống:

### 4.1. Cấu hình Docker Compose

*   `docker-compose.yml`: Tệp cấu hình chính cho Docker Compose, định nghĩa các dịch vụ, mạng và volume để phát triển cục bộ.
*   `docker-compose.prod.yml`: Tệp cấu hình Docker Compose dành cho môi trường sản phẩm, có thể chứa các thiết lập tối ưu hóa hiệu suất và bảo mật.

### 4.2. `nginx/` - Cấu hình Nginx

*   `default.conf`: Cấu hình cho máy chủ web Nginx, được sử dụng làm reverse proxy cho ứng dụng frontend và backend, xử lý cân bằng tải và phục vụ các tệp tĩnh.

### 4.3. `promtail-config.yaml` - Cấu hình Promtail

*   Cấu hình cho Promtail, một tác nhân thu thập nhật ký được sử dụng để gửi nhật ký từ các dịch vụ của ứng dụng đến Loki (hệ thống tổng hợp nhật ký).

### 4.4. `app/` - Cấu hình Ứng dụng Chung

*   (Cần được làm rõ thêm về nội dung cụ thể của thư mục này, ví dụ: scripts khởi tạo, cấu hình môi trường chung, v.v.)

## 5. Các Vấn đề Xuyên suốt (Cross-cutting Concerns)

### 5.1. Cơ sở dữ liệu

*   **Loại:** MySQL.
*   **Quản lý:** Entity Framework Core (cho backend) để tương tác với cơ sở dữ liệu, quản lý migrations và định nghĩa schema.

### 5.2. Xác thực và Ủy quyền (Authentication & Authorization)

*   **Phương thức:** JWT (JSON Web Tokens) Authentication được triển khai ở backend để bảo mật các API.
*   **Quản lý:** Backend chịu trách nhiệm phát hành và xác thực JWT.

### 5.3. Ghi nhật ký (Logging)

*   Hệ thống sử dụng các công cụ và cấu hình để thu thập, tổng hợp và phân tích nhật ký từ tất cả các ứng dụng và dịch vụ, hỗ trợ việc giám sát và gỡ lỗi. (Có thể sử dụng Promtail/Loki).

### 5.4. Hàng đợi Tin nhắn (Message Queue)

*   **Loại:** RabbitMQ.
*   **Mô tả:** Được sử dụng làm Message Broker để cho phép các microservice giao tiếp bất đồng bộ, giảm sự phụ thuộc lẫn nhau và tăng cường khả năng mở rộng. Ví dụ, Notification Service có thể sử dụng RabbitMQ để xử lý các yêu cầu gửi thông báo.

---
**Lưu ý:** Tài liệu này sẽ được cập nhật khi kiến trúc hệ thống phát triển.