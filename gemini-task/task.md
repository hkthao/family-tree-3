gemini-cli \
  --ask "
Tạo một Vue 3 component sử dụng D3.js để vẽ cây gia phả với các yêu cầu sau:

1. Dữ liệu:
   - Mỗi thành viên có id, name, avatar (URL), birthYear, deathYear, parents (mảng 0–2 ID).
   - Có thể mở rộng nhiều thế hệ.

2. Multiple parents:
   - Một người có thể có 2 cha mẹ.
   - Liên kết bằng cạnh từ cả hai parent đến node con.

3. UI:
   - Node hiển thị avatar (hình tròn), tên, năm sinh – năm mất.
   - Hover vào node: highlight node + edges liên quan.

4. Chức năng:
   - Ô tìm kiếm: nhập tên → highlight node và focus vào vị trí node.
   - Hỗ trợ zoom và pan trên sơ đồ.
   - Responsive, tự fit vào container.

5. Yêu cầu:
   - D3.js v6+.
   - TypeScript + Vue 3 SFC.
   - Dữ liệu mẫu để trong file JSON riêng.
   - Giải thích cách mở rộng thêm tính năng (ví dụ click node để mở chi tiết).
"
