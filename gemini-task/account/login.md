gemini generate ui --framework vue --library vuetify --task "
Thiết kế màn hình **Login** theo phong cách hiện đại, giống screenshot (Sneat Login Page).

**Yêu cầu UI:**
- Form Login nằm giữa màn hình, trong card bo góc, bóng nhẹ.
- Logo + tên app trên đầu (ví dụ: FamilyTree hoặc Sneat).
- Tiêu đề chào mừng: 'Welcome to [AppName]! 👋'
- Input Email:
  - Label: 'Email'
  - Placeholder: 'johndoe@email.com'
  - Có icon email ở đầu.
- Input Password:
  - Label: 'Password'
  - Có nút toggle show/hide password (eye icon).
- Checkbox 'Remember me'
- Link 'Forgot Password?' ở bên phải checkbox.
- Nút Login (full width, màu primary).
- Text 'New on our platform? Create an account' với link sang trang Register.
- Divider 'or'
- Social login buttons (Facebook, Twitter, GitHub, Google) với icon (mdi hoặc brand icon).

**Router yêu cầu:**
- `/login`: màn hình Login (LoginView.vue).
- `/register`: màn hình Register (RegisterView.vue, có thể để placeholder).
- `/dashboard`: màn hình chính sau khi đăng nhập (DashboardView.vue, placeholder).
- Khi login thành công (mock data check), điều hướng sang `/dashboard`.
- Nếu sai → hiển thị snackbar 'Invalid credentials'.

**Chi tiết kỹ thuật:**
- Vue 3 + Composition API.
- Vuetify 3: dùng v-app, v-main, v-container, v-row, v-col, v-card, v-text-field, v-btn, v-checkbox, v-divider, v-icon, v-snackbar.
- Responsive: mobile card chiếm gần full width.
- Tách component:
  - `LoginForm.vue`
  - `SocialLogin.vue`
- Router config trong `router/index.ts`.

**Kết quả mong muốn:**
- Source code Vue + Vuetify có thể chạy ngay bằng 'npm run dev'.
- Có router, mock login logic, UI giống screenshot."
