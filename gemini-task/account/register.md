gemini generate ui --framework vue --library vuetify --task "
Thiết kế màn hình **Đăng ký tài khoản (Sign Up)** theo phong cách hiện đại, giống screenshot (Sneat Register Page).

**Yêu cầu UI:**
- Card bo góc, bóng nhẹ, nằm giữa màn hình.
- Logo + tên app ở trên cùng.
- Tiêu đề: 'Adventure starts here 🚀'
- Subtext: 'Make your app management easy and fun!'

**Form fields:**
- Input Username (label: 'Username')
- Input Email (label: 'Email')
- Input Password (label: 'Password', có toggle show/hide eye icon).
- Checkbox 'I agree to privacy policy & terms' với link cho 'privacy policy & terms'.

**Actions:**
- Nút **SIGN UP** (full width, màu primary).
- Text 'Already have an account? Sign in instead' → link về màn hình Login.
- Divider 'or'
- Social login buttons (Facebook, Twitter, GitHub, Google) với icon (mdi hoặc brand icon).

**Router yêu cầu:**
- `/register`: màn hình Register (RegisterView.vue).
- `/login`: màn hình Login (LoginView.vue).
- `/dashboard`: màn hình chính (DashboardView.vue, placeholder).
- Khi đăng ký thành công (mock logic), chuyển hướng sang `/dashboard`.

**Chi tiết kỹ thuật:**
- Vue 3 + Composition API.
- Vuetify 3: dùng v-app, v-main, v-container, v-row, v-col, v-card, v-text-field, v-btn, v-checkbox, v-divider, v-icon, v-snackbar.
- Responsive: mobile card chiếm gần full width.
- Component tách riêng:
  - `RegisterForm.vue`
  - `SocialLogin.vue`
- Router config trong `router/index.ts`.

**Logic mock:**
- Chỉ cần validate input không rỗng.
- Nếu hợp lệ → hiển thị snackbar 'Account created successfully' → chuyển hướng `/dashboard`.
- Nếu lỗi → snackbar 'Please fill in all fields'.

**Kết quả mong muốn:**
- Source code Vue + Vuetify có thể chạy ngay bằng 'npm run dev'.
- UI giống screenshot, có router, có mock logic đăng ký."
