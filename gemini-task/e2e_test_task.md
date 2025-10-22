Bạn là một trợ lý kiểm thử chuyên nghiệp. Viết **mã E2E test bằng Playwright Test Runner với TypeScript** cho ứng dụng **Gia Phả** (Family Tree App) với các yêu cầu sau:

1. **Chỉ test các case quan trọng**, không cần cover 100% chức năng.  
   - Các case quan trọng gồm: 
     1. Đăng nhập bằng Auth0 với test account hợp lệ.  
     2. Tạo một cây gia phả mới.  
     3. Thêm thành viên (cha/mẹ/con) vào cây gia phả.  
     4. Cập nhật thông tin thành viên.  
     5. Xem chi tiết cây gia phả.  
     6. Xóa cây gia phả.  
     7. Đăng xuất.

2. **Chạy sequential thật sự:**  
   - Implement một test case, nếu pass thì mới chạy test case kế tiếp.  
   - Sử dụng `test.describe.serial` hoặc cách tương đương trong Playwright để đảm bảo tuần tự.  
   - Không tràn chạy nhiều test cùng lúc.

3. **Selector / test-id:**  
   - Luôn ưu tiên sử dụng `data-testid` để truy cập component, vì id hoặc tag có thể thay đổi theo implement.  
   - Nếu component chưa có `data-testid`, comment kiểu:  
     ```ts
     // TODO: cần thêm data-testid để truy cập component này và đảm bảo test ổn định
     ```  
   - Không nên dùng selector dựa trên class, id hoặc tag trực tiếp trừ khi không còn lựa chọn nào khác.

4. **Auth0 login:**  
   - Thực hiện login bằng UI với test account Auth0.  
   - Nếu cần mock token, ghi comment giải thích cách làm.

5. **Dựa trên implement thật và docs/**:  
   - Không bịa selector, API hay luồng nghiệp vụ mới.  
   - Tham khảo `docs/` để hiểu business rule và luồng dữ liệu.

6. **Comment bằng tiếng Việt chi tiết:**  
   - Giải thích mục đích từng bước, dữ liệu input và kỳ vọng output.  
   - Ví dụ:  
     ```ts
     // Kiểm tra bước đăng nhập: nhập email và mật khẩu hợp lệ
     // Nếu đăng nhập thành công thì chuyển sang trang dashboard
     ```

7. **Cấu trúc file test:**  
   - File TypeScript `.spec.ts`  
   - Đặt tên: `family-tree.e2e.spec.ts`  
   - Có setup/teardown hợp lý nếu cần reset state hoặc cleanup.

8. **Output:**  
   - Một file test hoàn chỉnh, có thể copy vào `tests/e2e/` và chạy trực tiếp với Playwright.

Hãy viết mã **E2E test hoàn chỉnh** theo hướng dẫn trên, có comment tiếng Việt, chạy tuần tự, ưu tiên `data-testid`, chỉ test các case quan trọng.
