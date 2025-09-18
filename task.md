gemini task create "Generate UI for family member search (list view)" --description "
Story: Là người dùng, tôi muốn tìm kiếm thành viên trong gia đình bằng danh sách.

Requirements:
- Tạo màn hình với ô tìm kiếm (search bar) để nhập tên hoặc ID thành viên.
- Hiển thị kết quả dưới dạng danh sách (list view), mỗi item có:
  - Họ tên
  - Năm sinh (hoặc tuổi)
  - Quan hệ trong gia đình (ví dụ: con, cha, mẹ, anh chị em)
- Khi bấm vào 1 item trong danh sách, mở trang chi tiết thành viên.
- Trang chi tiết hiển thị thông tin đầy đủ: họ tên, ngày sinh, quan hệ, và có nút xem cây gia phả (family tree view).
- Giao diện đơn giản, hiện đại, responsive (desktop/mobile).
- có mock data để demo.
- tham khảo frontend, readme 
" --output ui/family-search-list
