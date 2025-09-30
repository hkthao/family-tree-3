gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai màn hình Login.

### 1. Yêu cầu
- Tạo một màn hình đăng nhập thân thiện, hiện đại.
- Cung cấp các trường nhập liệu cho email và mật khẩu.
- Có các tính năng phụ như "Remember me", "Forgot Password?", và đăng nhập bằng mạng xã hội.
- Điều hướng người dùng đến trang dashboard sau khi đăng nhập thành công.

### 2. Thiết kế
- **Layout:**
  - Form Login nằm giữa màn hình, trong một `v-card` có bo góc và đổ bóng nhẹ.
- **Nội dung:**
  - **Logo và Tên ứng dụng:** Hiển thị ở đầu form.
  - **Tiêu đề chào mừng:** "Welcome to [AppName]! 👋"
  - **Input Email:**
    - Label: "Email"
    - Placeholder: "johndoe@email.com"
    - Có icon email ở đầu.
  - **Input Password:**
    - Label: "Password"
    - Có nút toggle show/hide password (eye icon).
  - **Checkbox "Remember me"**
  - **Link "Forgot Password?"** ở bên phải checkbox.
  - **Nút Login:** Full width, màu primary.
  - **Link đăng ký:** "New on our platform? Create an account" với link sang trang Register.
  - **Divider "or"**
  - **Nút đăng nhập mạng xã hội:** Facebook, Twitter, GitHub, Google với icon.

### 3. Kỹ thuật chung
- **Component:** `LoginView.vue`
- **Framework:** Vue 3 + Composition API
- **UI Library:** Vuetify 3
- **Routing:**
  - `/login`: Màn hình Login.
  - `/register`: Màn hình Register.
  - `/dashboard`: Màn hình chính sau khi đăng nhập.
- **Logic:**
  - Khi login thành công (mock data check), điều hướng sang `/dashboard`.
  - Nếu sai → hiển thị snackbar "Invalid credentials".
- **Tách component:**
  - `LoginForm` (từ `@/components/auth`)
  - `SocialLogin` (từ `@/components/auth`)

### Kết quả mong muốn
- Source code cho `LoginView.vue` và các component con.
- Trang Login hiển thị đúng và có mock login logic."