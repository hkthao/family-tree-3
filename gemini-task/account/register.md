gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai màn hình Đăng ký tài khoản (Sign Up).

### 1. Yêu cầu
- Tạo một màn hình đăng ký tài khoản thân thiện, hiện đại.
- Cung cấp các trường nhập liệu cho username, email, và mật khẩu.
- Có các tính năng phụ như đồng ý với điều khoản dịch vụ và đăng nhập bằng mạng xã hội.
- Điều hướng người dùng đến trang dashboard sau khi đăng ký thành công.

### 2. Thiết kế
- **Layout:**
  - Form Đăng ký nằm giữa màn hình, trong một `v-card` có bo góc và đổ bóng nhẹ.
- **Nội dung:**
  - **Logo và Tên ứng dụng:** Hiển thị ở đầu form.
  - **Tiêu đề:** "Adventure starts here 🚀"
  - **Subtext:** "Make your app management easy and fun!"
  - **Input Username:** Label: "Username"
  - **Input Email:** Label: "Email"
  - **Input Password:** Label: "Password", có nút toggle show/hide password (eye icon).
  - **Checkbox "I agree to privacy policy & terms"** với link cho "privacy policy & terms".
  - **Nút SIGN UP:** Full width, màu primary.
  - **Link đăng nhập:** "Already have an account? Sign in instead" → link về màn hình Login.
  - **Divider "or"**
  - **Nút đăng nhập mạng xã hội:** Facebook, Twitter, GitHub, Google với icon.

### 3. Kỹ thuật chung
- **Component:** `RegisterView.vue`
- **Framework:** Vue 3 + Composition API
- **UI Library:** Vuetify 3
- **Routing:**
  - `/register`: Màn hình Register.
  - `/login`: Màn hình Login.
  - `/dashboard`: Màn hình chính sau khi đăng nhập.
- **Logic:**
  - Khi đăng ký thành công (mock logic), hiển thị snackbar "Account created successfully" và chuyển hướng sang `/dashboard`.
  - Nếu lỗi → snackbar "Please fill in all fields".
- **Tách component:**
  - `RegisterForm.vue`
  - `SocialLogin.vue`

### Kết quả mong muốn
- Source code cho `RegisterView.vue` và các component con.
- Trang Register hiển thị đúng và có mock logic đăng ký."