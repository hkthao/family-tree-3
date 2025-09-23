Bạn là một senior frontend engineer. Hãy tạo hoặc refactor component Vue 3 dùng chung tên `Lookup.vue` theo các yêu cầu sau:

1. Component dùng Vuetify 3 `v-select`.
2. Props:
   - `dataSource`: có thể là Array<any> hoặc Pinia store. Store có thể chứa danh sách data hoặc method fetch data.
   - `value`: id hiện tại (string | number | null).
   - `displayExpr`: field để hiển thị label (ví dụ "name").
   - `valueExpr`: field dùng làm value (ví dụ "id").
3. Behavior:
   - `v-select` hiển thị `value` dựa trên `displayExpr`.
   - Khi click nút search (icon bên cạnh v-select), mở dialog (`v-dialog`) chứa danh sách:
       - v-list show dữ liệu từ `dataSource`.
       - On-demand search: nếu `dataSource` là store, gọi method fetch / ensureLoaded.
       - Phân trang
       - Cho phép sort / filter cơ bản.
   - **Preload selected item**: nếu `value` đã có nhưng `items` chưa load, fetch riêng item đó để hiển thị label trong v-select.
   - Khi chọn item trong dialog → cập nhật `value` của v-select.
4. Component phải hỗ trợ binding 2 chiều `v-model:value`.
5. Code bằng TypeScript.
6. Comment rõ ràng để junior dễ hiểu.
7. Đảm bảo component reusable với nhiều store hoặc array, và on-demand load vẫn hiển thị label của selected value ngay cả khi items chưa load.
