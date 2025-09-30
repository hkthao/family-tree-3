Tạo một component Vue 3 + TypeScript tên là AvatarInput.vue với các yêu cầu sau:

1. Chức năng:
   - Cho phép người dùng chọn avatar theo 2 cách:
     a) Nhập URL hình ảnh.
     b) Upload hình ảnh từ máy tính.
   - Nếu upload hình ảnh:
     - Mở cropper trước khi lưu (dùng thư viện cropper như cropperjs hoặc vue-advanced-cropper).
     - Sau khi crop, giảm dung lượng bằng cách resize và export ra base64 hoặc blob.
   - Sau khi chọn (URL hoặc crop), emit sự kiện `update:src` để truyền dữ liệu hình ảnh cho component cha.

2. UI:
   - Dùng Vuetify `v-card` hoặc `v-sheet` để chứa form.
   - Có 2 tab (`v-tabs`):
     - Tab 1: "Dán URL"
       - Input (`v-text-field`) cho phép nhập link hình ảnh.
       - Nút "Xác nhận" để emit link này.
     - Tab 2: "Tải ảnh lên"
       - Nút `v-file-input` để chọn ảnh từ máy tính.
       - Khi ảnh được chọn, hiển thị cropper trong modal/dialog.
       - Có nút "Lưu" trong cropper để export ảnh crop và emit.

3. Props:
   - `modelValue: string | null` (ảnh hiện tại).
   - `size?: number` (kích thước preview, mặc định 128).

4. Emits:
   - `update:modelValue` (trả về URL hoặc base64 sau khi chọn).

5. Preview:
   - Luôn hiển thị avatar preview bằng cách tái sử dụng component `AvatarDisplay.vue`.
   - Preview cập nhật theo giá trị `modelValue`.

6. Yêu cầu code:
   - Vue 3 + `<script setup lang="ts">`.
   - Dùng Vuetify components (`v-avatar`, `v-tabs`, `v-file-input`, `v-dialog`, `v-btn`, `v-img`).
   - Có integration với thư viện cropper (cropperjs hoặc vue-advanced-cropper).
   - Code rõ ràng, có comment giải thích.
