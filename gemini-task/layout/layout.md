gemini generate ui --framework vue --library vuetify --task "
Thiết kế layout dashboard theo phong cách hiện đại giống screenshot (Sneat Admin Template):

**Yêu cầu layout tổng thể:**
- **Sidebar trái (Navigation Drawer)**:
  - Logo / tên app trên đầu.
  - Các menu: Dashboards, Front Pages, Email, Chat, Calendar, Kanban, Account Settings, Login, Register.
  - Một số menu có badge 'Pro'.
  - Dùng v-list + v-list-item + v-badge.

- **Top App Bar**:
  - Search box ở giữa (có icon search).
  - Nút toggle dark/light mode.
  - Notification bell icon.
  - Avatar người dùng bên phải.

- **Content chính (Main Panel)**:
  - Greeting card (Congratulations John! ...).
  - Grid hiển thị:
    - Biểu đồ cột (Total Revenue, năm 2023/2024).
    - Gauge/Progress circular (78% Growth).
    - Các card nhỏ: Profit, Sales, Payments, Transactions.
    - Card Profile Report (line chart).
    - Card Order Statistics.
    - Card Transactions.

**Chi tiết kỹ thuật:**
- Dùng Vue 3 + Composition API.
- UI sử dụng Vuetify 3 (v-app, v-navigation-drawer, v-app-bar, v-main, v-container, v-row, v-col, v-card, v-chart mock).
- Dùng v-data-table, v-pagination nếu cần hiển thị dữ liệu danh sách.
- Biểu đồ mock có thể dùng chart placeholder component (hoặc fake chart bằng Vuetify progress).
- Có responsive: sidebar thu gọn khi màn hình nhỏ.
- Code chia component: Sidebar.vue, TopBar.vue, Dashboard.vue, StatisticCard.vue, ChartCard.vue.
- Mock data (Profit, Sales, Payments...) để hiển thị demo.

Kết quả mong muốn: Source code Vue + Vuetify dashboard layout giống hình, có thể chạy ngay bằng 'npm run dev'."
