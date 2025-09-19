gemini generate ui --framework vue --library vuetify --task "
Thiết kế layout dashboard hiện đại theo phong cách Sneat Admin Template (Google/IBM style):

**Sidebar (Navigation Drawer)**:
- Nền trắng, border-right mảnh.
- Logo + tên app trên đầu.
- Menu items: Dashboards, Front Pages, Email, Chat, Calendar, Kanban, Account Settings, Login, Register.
- Một số menu có badge 'Pro' (v-badge color='purple' rounded).
- Menu có icon + text, density='comfortable'.
- Nhóm menu bằng v-list-subheader (APPS & PAGES, USER INTERFACE, FORMS & TABLES).

**Top App Bar**:
- Nền trắng, elevation nhẹ.
- Search box lớn (v-text-field, prepend-inner-icon='mdi-magnify', append-inner='⌘K', rounded-lg).
- Dark/Light mode toggle.
- Notification bell có badge.
- Avatar người dùng (v-avatar size='36').

**Main Content**:
- Dùng v-container fluid + v-row + v-col.
- Greeting card: có illustration, text 'Congratulations John!', button 'View Badges'.
- Grid layout với card bo góc lớn (rounded-xl), elevation-2:
  - Total Revenue (bar chart, 2023 vs 2024).
  - Growth circular progress (78%).
  - Stat cards nhỏ: Profit, Sales, Payments, Transactions (icon trong background nhạt).
  - Profile Report (line chart, màu vàng).
  - Order Statistics.
  - Transactions (có CTA 'Upgrade to Pro' gradient).
- Hover card có transition nâng nhẹ.

**Typography & Spacing**:
- Font: text-h5 cho title, text-subtitle-1 cho subtitle, text-caption cho chi tiết.
- Khoảng cách giữa card = 24px (gutter).
- Padding card = 16px.

**Interaction & Feedback**:
- Icon action có tooltip.
- Snackbar cho CRUD hoặc action.
- Dialog confirm khi xoá.

**Technical details**:
- Vue 3 + Composition API.
- Vuetify 3 (v-app, v-navigation-drawer, v-app-bar, v-container, v-card, v-data-table, v-pagination, v-tooltip, v-dialog, v-snackbar).
- Chart: mock bằng progress hoặc placeholder component.
- Responsive: sidebar mini-variant trên mobile.
- Components: Sidebar.vue, TopBar.vue, Dashboard.vue, StatisticCard.vue, ChartCard.vue, FamilyManagement.vue.

**Theme**:
- Primary: #696CFF (giống Sneat).
- Secondary: #8592A3.
- Success: #71DD37.
- Error: #FF3E1D.
- Warning: #FFAB00.

Kết quả mong muốn: Source code Vue + Vuetify dashboard layout giống Sneat, có thể chạy ngay bằng 'npm run dev'."
