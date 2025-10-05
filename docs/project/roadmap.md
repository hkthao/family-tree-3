# Lộ trình Phát triển (Roadmap)

## Mục lục

- [1. Giới thiệu](#1-giới-thiệu)
- [2. Mục tiêu Ngắn hạn (1-3 tháng)](#2-mục-tiêu-ngắn-hạn-1-3-tháng)
- [3. Mục tiêu Trung hạn (3-6 tháng)](#3-mục-tiêu-trung-hạn-3-6-tháng)
- [4. Mục tiêu Dài hạn (6-12 tháng)](#4-mục-tiêu-dài-hạn-6-12-tháng)

---

## 1. Giới thiệu

Tài liệu này phác thảo **Lộ trình Phát triển (Roadmap)** cho dự án Cây Gia Phả, định hướng các mục tiêu và tính năng chính sẽ được phát triển trong các quý tiếp theo. Lộ trình này được xây dựng dựa trên các ưu tiên đã xác định trong [Product Backlog](./backlog.md) và tập trung vào việc cung cấp giá trị gia tăng cho người dùng theo từng giai đoạn. Nó giúp toàn bộ đội ngũ phát triển và các bên liên quan có cái nhìn tổng quan về hướng đi của sản phẩm.

## 2. Mục tiêu Ngắn hạn (1-3 tháng) (updated after refactor)

Trong giai đoạn ngắn hạn, chúng tôi tập trung vào việc hoàn thiện các chức năng cốt lõi và cải thiện trải nghiệm người dùng cơ bản:

*   **Hoàn thiện chức năng quản lý Dòng họ và Thành viên**: (Đã hoàn thành)
    *   Thêm, sửa, xóa thông tin dòng họ và thành viên.
    *   Xem chi tiết thông tin thành viên và dòng họ.
    *   Tìm kiếm cơ bản thành viên và dòng họ.
*   **Quản lý sự kiện**: (Đã hoàn thành)
    *   Thêm, sửa, xóa các sự kiện quan trọng của gia đình (sinh, kết hôn, mất, họp mặt).
    *   Xem dòng thời gian các sự kiện.
*   **Cải thiện cây gia phả**: 
    *   Thêm các kiểu hiển thị mới cho cây gia phả (ví dụ: sơ đồ dọc, ngang).
    *   Chức năng phóng to/thu nhỏ và di chuyển trên cây gia phả.
    *   Khả năng thay đổi nút gốc của cây gia phả.
*   **Tăng cường hiệu năng**: 
    *   Tối ưu hóa tốc độ tải và hiển thị dữ liệu, đặc biệt là với cây gia phả lớn.
    *   Cải thiện hiệu suất API Backend.
*   **Xử lý lỗi và thông báo**: 
    *   Triển khai `Result Pattern` nhất quán ở Backend.
    *   Hiển thị thông báo lỗi thân thiện và rõ ràng ở Frontend.

## 3. Mục tiêu Trung hạn (3-6 tháng)

Trong giai đoạn trung hạn, chúng tôi sẽ mở rộng các tính năng cốt lõi và tích hợp các công nghệ mới:

*   **Tính năng cộng tác**: 
    *   Cho phép người dùng mời các thành viên khác cùng quản lý một cây gia phả.
    *   Phân quyền chi tiết cho từng thành viên được mời (ví dụ: chỉ xem, chỉnh sửa, quản lý).
    *   Hệ thống thông báo khi có thay đổi trong cây gia phả được chia sẻ.
*   **Xuất/Nhập dữ liệu**: 
    *   Hỗ trợ chuẩn GEDCOM (Genealogical Data Communication) để xuất và nhập dữ liệu gia phả, cho phép người dùng dễ dàng di chuyển dữ liệu giữa các hệ thống khác nhau.
    *   Xuất dữ liệu sang các định dạng phổ biến khác như PDF, CSV.
*   **Tích hợp AI (Giai đoạn 1)**: 
    *   Gợi ý nội dung tiểu sử dựa trên dữ liệu thành viên đã có sẵn.
    *   Nhận diện khuôn mặt từ ảnh để tự động gắn thẻ thành viên.

## 4. Mục tiêu Dài hạn (6-12 tháng)

Trong giai đoạn dài hạn, chúng tôi hướng tới việc mở rộng hệ thống với các tính năng nâng cao và cải thiện trải nghiệm người dùng toàn diện:

*   **Báo cáo và thống kê**: 
    *   Cung cấp các báo cáo chi tiết và biểu đồ thống kê về gia phả (ví dụ: phân bố tuổi, giới tính, số lượng thành viên theo thế hệ).
    *   Tính năng tạo báo cáo tùy chỉnh và xuất báo cáo.
*   **Tích hợp mạng xã hội**: 
    *   Cho phép người dùng chia sẻ thông tin gia phả (có kiểm soát quyền riêng tư) lên các nền tảng mạng xã hội.
    *   Tích hợp đăng nhập nhanh bằng tài khoản mạng xã hội (Google, Facebook).
*   **Hỗ trợ đa ngôn ngữ**: 
    *   Mở rộng ứng dụng để hỗ trợ nhiều ngôn ngữ khác nhau, bắt đầu với tiếng Việt và tiếng Anh.
    *   Cung cấp giao diện để người dùng dễ dàng chuyển đổi ngôn ngữ.
*   **Tích hợp AI (Giai đoạn 2)**: 
    *   Tìm kiếm thành viên bằng khuôn mặt từ ảnh.
    *   Phân tích dữ liệu gia phả để đưa ra các insight thú vị.
*   **Cải thiện trải nghiệm người dùng**: 
    *   Tối ưu hóa giao diện người dùng, thêm các hiệu ứng động và tương tác mượt mà hơn.
    *   Cải thiện khả năng truy cập (accessibility) cho người dùng khuyết tật.