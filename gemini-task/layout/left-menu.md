gemini generate ui --framework vue --library vuetify --task "
Mục tiêu:
Tạo full-featured **Left Sidebar (Navigation Drawer)** cho ứng dụng quản lý gia phả. Sidebar phải đầy đủ chức năng cho cả Admin và User, theo backlog đã cung cấp (file backlog.md). Thiết kế giống phong cách Sneat Admin Template (white sidebar, subtle borders, rounded cards), dùng Vue 3 + Composition API + Vuetify 3. Sản phẩm đầu ra là code sẵn chạy được (npm run dev).

YÊU CẦU CHUNG (kĩ thuật)
- Vue 3 + Composition API + TypeScript (nếu project ko dùng TS thì tạo JS, ưu tiên TS).
- Vuetify 3 components: v-navigation-drawer, v-list, v-list-item, v-list-subheader, v-badge, v-icon, v-tooltip.
- Folder output: /components/layout/Sidebar.vue, /components/layout/TopBar.vue, /data/menuItems.ts, /router/sidebar-routes.ts, /utils/menu-permissions.ts
- Kèm i18n keys (/locales/en.json, /locales/vi.json) cho mỗi menu label.
- Responsive: drawer có mini-variant trên mobile, auto-collapse below 1024px.
- Persist collapsed state to localStorage (key: app.sidebarCollapsed).
- Active route highlight, expand nested groups when active.
- Search within menu (typed search filter), shortcut to open search (Ctrl/Cmd+K).
- Keyboard navigation: up/down/enter to open item, Esc to close drawer (ARIA support).
- Accessibility: aria-labels, role='navigation', focus management for dialog.
- Theming: use Vuetify theme tokens; config suggestion: primary #696CFF, secondary #8592A3, success #71DD37, error #FF3E1D.

MAPPING MENU (Dựa trên backlog.md)
- Tách theo section (v-list-subheader): Dashboards, Gia phả, Hồ sơ & Nội dung, Quản trị, Tiện ích & AI, Văn hóa & Truyền thống, Hệ thống.
- Mỗi item có: title, icon (MDI), to (route), roles (array of allowed roles), badge? (object), children? (sub-menu).
- Roles model: ['Admin','FamilyManager','Editor','Viewer','Guest'].
- Nếu item visible cho tất cả thì omit roles or roles: ['Admin','FamilyManager','Editor','Viewer'].

CHI TIẾT MENU (cung cấp sẵn — Gemini phải generate chính xác structure)
- Dashboards
  - Tổng quan — icon: mdi-view-dashboard, to: /dashboard, roles: all
- Gia phả
  - Xem cây gia phả — mdi-family-tree, /family/tree, roles: all
  - Thêm thành viên — mdi-account-plus, /family/add, roles: ['Admin','FamilyManager','Editor']
  - Quản lý thành viên — mdi-account-multiple, /family/members, roles: ['Admin','FamilyManager','Editor']
  - Sự kiện gia đình (Timeline) — mdi-timeline-text, /family/timeline, roles: all
  - Thay đổi nút gốc cây — mdi-target, /family/reroot, roles: ['Admin','FamilyManager','Editor']
  - In / Xuất (PDF, GEDCOM) — mdi-printer, /family/export, roles: ['Admin','FamilyManager']
- Hồ sơ & Nội dung
  - Hồ sơ của tôi — mdi-account, /profile, roles: all
  - Cập nhật thông tin — mdi-account-edit, /profile/edit, roles: all
  - Ghi chú & Tài liệu — mdi-file-document-multiple, /profile/attachments, roles: ['Admin','FamilyManager','Editor']
  - Ghi âm / Ký ức giọng nói — mdi-microphone, /profile/voice, roles: all
  - AI gợi ý tiểu sử — mdi-robot, /ai/bio-suggest, roles: ['Admin','FamilyManager','Editor']
- Quản trị (Admin)
  - Quản lý người dùng — mdi-account-cog, /admin/users, roles: ['Admin']
  - Quản lý vai trò & quyền — mdi-shield-account, /admin/roles, roles: ['Admin']
  - Nhật ký thay đổi (Audit log) — mdi-file-document, /admin/audit, roles: ['Admin']
  - Mời thành viên — mdi-email-plus, /admin/invite, roles: ['Admin','FamilyManager']
  - Multi-tree & Ghép nối — mdi-sitemap, /admin/multi-tree, roles: ['Admin','FamilyManager']
  - Phát hiện trùng lặp — mdi-magnify-remove-outline, /admin/duplicates, roles: ['Admin','FamilyManager']
- Tiện ích & AI
  - Tìm kiếm thông minh (NL query) — mdi-magnify, /search/smart, roles: all
  - Tìm kiếm mối quan hệ — mdi-link-variant, /search/relationship, roles: all
  - Nhận diện khuôn mặt (tagging) — mdi-face-recognition, /ai/face-tag, roles: ['Admin','FamilyManager']
  - Tìm kiếm bằng khuôn mặt — mdi-image-search, /search/face, roles: ['Admin','FamilyManager']
  - Chatbot AI — mdi-chat, /ai/chatbot, roles: all
  - Cộng tác real-time — mdi-account-group, /realtime/collab, roles: ['Admin','FamilyManager','Editor']
- Văn hóa & Truyền thống
  - Truyền thống & lễ hội — mdi-calendar-star, /culture/traditions, roles: all
  - Thông báo ngày giỗ/ kỷ niệm — mdi-bell-ring, /notifications/anniversaries, roles: all
- Hệ thống
  - Cài đặt hệ thống — mdi-cog, /settings/system, roles: ['Admin']
  - Cài đặt tài khoản — mdi-lock, /settings/account, roles: all
  - Xuất/Nhập dữ liệu — mdi-database-export, /settings/import-export, roles: ['Admin','FamilyManager']
  - Báo cáo & Thống kê — mdi-chart-box, /reports, roles: ['Admin','FamilyManager','Editor','Viewer']

UI / UX Behavior
- Groups must use v-list-subheader with section label and optional helper description.
- Support nested children (max 2 levels): show chevron expand icon, animate expand/collapse.
- Show badge counts if provided (e.g., pending invites: show badge '5' on Mời thành viên).
- Highlight active item: colored left indicator (4px) and primary background lighten.
- Hover style: elevation + subtle bg color (use Vuetify variant).
- Provide top of sidebar small search input to quickly filter menu items by label.
- Provide a 'Favorites' quick-access section (user can star items). Save favorites to localStorage.
- Provide ability to collapse sidebar to icons-only (mini-variant). Provide tooltip on hover for collapsed icons.
- Provide keyboard shortcut to toggle collapse (Ctrl/Cmd+B).
- Provide option to hide admin-only sections when user role lacks permission.

Role & Permission Implementation (deliverables)
- /utils/menu-permissions.ts: export function canAccessMenu(userRoles: string[], itemRoles?: string[]) -> boolean.
- Add meta to routes: meta: { roles: ['Admin','FamilyManager'] } and route guard sample snippet to check before each route.
- Sidebar should accept `currentUser` prop or read from global store (pinia / vuex) to filter visible items.

i18n & translations
- Provide /locales/vi.json and /locales/en.json keys for all menu labels, descriptions and tooltips. Example:
  - dashboard.overview = 'Tổng quan'
  - family.view = 'Xem cây gia phả'
  - admin.users = 'Quản lý người dùng'
- Sidebar should use $t('...') for labels.

Files to generate (exact path & minimal content)
1. /data/menuItems.ts
   - export default MenuItem[] with full structure described above (titleKey, icon, to, roles, badge?, children?).
   - include sample counts in badge (e.g., pendingInvites: 3).
2. /components/layout/Sidebar.vue
   - Fully functional Vuetify 3 component implementing all behaviors: groups, expand/collapse, search, favorites, permissions, persistence.
   - Read menuItems from /data/menuItems.ts.
   - Emit events for route navigation.
3. /components/layout/TopBar.vue
   - includes menu toggle button, search (Ctrl/Cmd+K binding), user avatar, notification bell (sample badge).
4. /router/sidebar-routes.ts
   - export array of route definitions mapped to menu items (with meta.roles).
5. /utils/menu-permissions.ts
6. /locales/vi.json and /locales/en.json
7. /tests/Sidebar.spec.ts (unit tests)
   - Use vitest or jest + @vue/test-utils: test render by role, test search filter, test collapse persistence, test favorites.
8. README snippet: how to integrate (import Sidebar, pass currentUser, add to App.vue).
9. Optional: /styles/sidebar-overrides.css for Sneat look (thin border, rounded, hover shadows).

Coding conventions & extras
- Use <script setup> and defineProps/defineEmits.
- Keep components small and well commented.
- Add TODO placeholders where server API would supply counts/ badges.
- For any icon not obvious, use closest MDI icon and list mapping.
- Provide one example user object for mocking currentUser: { id: 'u1', name: 'John', roles: ['FamilyManager'] }.

Testing & QA
- Unit tests: verify menu filtering by role, favorites saved & loaded from localStorage, keyboard toggle, search filtering.
- Accessibility check: ensure aria-labels for nav, buttons, and invalid state messages.

Dev output expectations
- Create a PR-style summary at the end: list files created and short explanation for each.
- Include instructions to run tests and start dev server.

Extra instruction for Gemini:
- Read backlog.md in repository and ensure each backlog feature is represented by at least one menu entry. If backlog has additional pages not covered above, add them under appropriate sections.
- If any ambiguities on role mapping or route naming, create a small mapping file `/data/backlog-to-menu-map.md` within output containing assumptions.

End of task.
"
