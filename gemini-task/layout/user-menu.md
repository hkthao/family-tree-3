gemini generate ui --framework vue --library vuetify --task "
Mục tiêu:
Tạo component **UserMenu** (avatar dropdown) giống hoàn chỉnh như hình chụp: avatar tròn ở top-right, khi click mở menu popup có header hiển thị avatar lớn, tên người dùng, vai trò (Admin) với badge, sau đó danh sách item: Profile, Settings, Pricing, FAQ, divider, Logout. Có indicator trạng thái online (green dot) chồng lên avatar. Thiết kế phải theo Vuetify 3, responsive, accessible, và sẵn sàng tích hợp vào TopBar.

YÊU CẦU KĨ THUẬT (bắt buộc)
- Framework: **Vue 3 + Composition API**. 
- Language: **TypeScript** (nếu repo không dùng TS thì tạo JS nhưng ưu tiên TS).
- UI library: **Vuetify 3** (v-menu, v-avatar, v-list, v-list-item, v-badge, v-divider, v-icon, v-tooltip, v-sheet, v-btn, v-dialog).
- Output files (tạo chính xác):
  - `/components/layout/UserMenu.vue` (component chính).
  - `/components/layout/UserMenu.types.ts` (interface type cho User).
  - `/data/userMock.ts` (1 object mock currentUser).
  - `/styles/user-menu-overrides.css` (các CSS override nhỏ cho look-and-feel Sneat).
  - `/tests/UserMenu.spec.ts` (unit tests bằng vitest + @vue/test-utils).
  - `/locales/en.json` và `/locales/vi.json` (i18n keys cho mọi label + tooltip).
  - `README_USERMENU.md` (hướng dẫn ngắn: cách import vào TopBar, props, events).
- Component phải dùng `<script setup lang='ts'>` + `defineProps`/`defineEmits`.
- Component props:
  - `currentUser: { id: string; name: string; role: string; avatarUrl?: string; online?: boolean }` (required).
  - `notificationsCount?: number` (optional).
  - `placement?: 'bottom' | 'bottom-end' | 'bottom-start'` (default 'bottom-end').
- Component emits:
  - `navigate` (payload: string route) — khi chọn menu item.
  - `logout` — khi user nhấn Logout (sau confirm).
  - `openSettings` — khi nhấn Settings.
- Route integration: nếu `to` prop provided on item, component emits `navigate` with that route.

UI / UX Behavior chi tiết (bắt buộc)
- **Activator**: avatar tròn (v-avatar) 36px, khi hover có slight elevation; có small green online dot overlay ở bottom-right (size ~10px) nếu `currentUser.online === true`.
- **Popup**: sử dụng `v-menu` with `activator` slot; content is a `v-sheet` with rounded corners (rounded-lg), elevation-3, min-width 220px, max-width 320px.
- **Header** (top of popup):
  - Avatar lớn 56px + user name (text-h6) + role (caption, muted) ngay bên phải.
  - Role hiển thị kèm pill/badge nhẹ (e.g., 'Admin' with variant tonal primary).
  - Header background separated by thin divider from menu list.
- **Menu items**:
  - Items: Profile, Settings, Pricing, FAQ. Mỗi item có icon MDI (Profile: mdi-account, Settings: mdi-cog, Pricing: mdi-currency-usd, FAQ: mdi-help-circle).
  - Use `<v-list>` + `<v-list-item>`; each `<v-list-item>` clickable; show tooltip text on collapsed/mini mode.
  - Add `v-tooltip` on each icon for better discoverability.
  - Add keyboard navigation (Up/Down to move, Enter to activate). Pressing Esc closes menu and returns focus to avatar.
- **Divider + Logout**:
  - After main items show `<v-divider>`.
  - Logout as last item with icon mdi-logout; style it with color='error' and bind click to open a confirm dialog.
  - Confirm logout: use `v-dialog` modal with message \"Bạn có chắc muốn đăng xuất?\" and buttons Cancel / Logout (Logout colored error). On confirm emit `logout`.
- **Accessibility**:
  - `role="navigation"` on menu container, aria-label localized e.g., $t('userMenu.ariaLabel').
  - Use `aria-haspopup`, `aria-expanded` on activator.
  - Manage focus: when menu opens focus first interactive element (Profile); when close return focus to avatar.
- **Keyboard Shortcuts**:
  - Support opening menu with Enter/Space on focused avatar.
  - Optional global shortcut `Ctrl/Cmd+U` to open UserMenu (configurable).
- **Mobile behavior**:
  - On small screens (<600px) present menu as bottom sheet (v-bottom-sheet / full-screen dialog) with same content, easier tap targets.
- **Theme & style**:
  - Use Vuetify theme tokens; follow colors similar to Sneat (primary: #696CFF).
  - Provide `styles/user-menu-overrides.css` to tune shadows, border-radius, online-dot styles, small badge.
  - Support light/dark mode (auto adapt colors).
- **Avatar fallback**:
  - If no `avatarUrl`, show initials (first letters of name) inside avatar with background color computed from user id (deterministic).
  - Provide small camera/upload mock button in header (non-functional placeholder with TODO comment).
- **i18n**:
  - Labels: userMenu.profile, userMenu.settings, userMenu.pricing, userMenu.faq, userMenu.logout, userMenu.confirmLogout, userMenu.cancel.
  - Provide en.json + vi.json mapping.

Menu data & behavior requirements
- Hardcode menu list inside component or import from `/data/userMenuItems.ts`:
  - Header info derived from `currentUser`.
  - Items array:
    ```ts
    [
      { key: 'profile', labelKey: 'userMenu.profile', icon: 'mdi-account', to: '/profile' },
      { key: 'settings', labelKey: 'userMenu.settings', icon: 'mdi-cog', to: '/settings' },
      { key: 'pricing', labelKey: 'userMenu.pricing', icon: 'mdi-currency-usd', to: '/pricing' },
      { key: 'faq', labelKey: 'userMenu.faq', icon: 'mdi-help-circle', to: '/faq' }
    ]
    ```
  - Logout item separated, `key: 'logout'`.
- If `notificationsCount` prop > 0, show small badge count over avatar (top-left or over bell icon depending ui).
- Provide optional `favorites` toggles per user menu (star) — if user stars Profile it becomes pinned in TopBar quick access; favorites persist to localStorage.

Tests (deliverables)
- `/tests/UserMenu.spec.ts` (vitest + @vue/test-utils) should cover:
  - Renders avatar and online dot when `online=true`.
  - Opens menu on click; header shows name and role from `currentUser`.
  - Emits `navigate` with correct route when clicking 'Profile'.
  - Shows confirm dialog on logout click; emits `logout` only after confirm.
  - Keyboard navigation opens/closes menu and triggers items.
  - Mobile variant renders bottom sheet when window width small (mock resize).
- Provide sample test runner commands in README (`pnpm/vite/npm`).

README & Integration notes
- `/README_USERMENU.md` include:
  - How to import `<UserMenu :currentUser='user' :notificationsCount='3' @navigate='onNav' @logout='onLogout' />`.
  - Example `currentUser` mock:
    ```ts
    export const mockUser = { id: 'u1', name: 'John Doe', role: 'Admin', avatarUrl: '/images/john.jpg', online: true };
    ```
  - How to wire navigate event to `router.push`.
  - How to toggle dark mode / mobile bottom sheet behavior.
  - How to run tests.

Styling details (exact)
- Popup `v-sheet` classes:
  - `class="user-menu-sheet rounded-lg elevation-3 pa-3"`
  - Header inside: `display:flex; align-items:center; gap:12px; padding-bottom:8px;`
  - Name: `class="text-h6 font-medium"`, role: `class="text-caption text--secondary"`.
  - Online dot: `position:absolute; right:6px; bottom:6px; border: 2px solid white` to create ring.
- Provide CSS variables for sizes and radii at top of `/styles/user-menu-overrides.css`.

Extra features (bonus — include them in gen)
- Generate Storybook story `/stories/UserMenu.stories.ts` (optional).
- Provide a minimal `TopBar.vue` sample showing how UserMenu integrates (activator placed in app bar).
- Provide `aria` attributes and comments explaining accessibility choices.

Quality & PR output
- Gemini should output a short PR-style summary at the end listing generated files and a 2–3 line description for each.
- Add TODO comments where API integration is required (e.g., logout endpoint, profile image upload).

Important:
- Read backlog.md and ensure user menu items like 'Mời thành viên' or 'Profile' are linked appropriately if backlog references them; otherwise use routes in menu data as provided above.
- If project is JavaScript-only, auto-fallback to JS but keep code structure identical.

End of task.
"
