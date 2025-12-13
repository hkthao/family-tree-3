
---

# 📘 TESTING GUIDELINE FOR VUE 3 (SMALL TEAM)

## 🎯 Mục tiêu

* Ưu tiên **độ ổn định & regression safety**
* Tránh viết test dư thừa
* Tối ưu cho **team nhỏ, deadline nhanh**

---

## 1️⃣ Nguyên tắc tổng quát

### 🔹 Triết lý

* **Test logic, không test UI**
* **Composable & Store là nơi test chính**
* `.vue` component **chỉ test khi quyết định flow**

### 🔹 KHÔNG theo đuổi

* 100% test coverage
* Snapshot test UI
* Test các component chỉ render template

---

## 2️⃣ Quy tắc cho COMPOSABLE

### ✅ Khi viết test cho composable

Composable **BẮT BUỘC** viết test nếu có ít nhất 1 yếu tố:

* Logic điều kiện
* Side effect (API, router, store, timer)
* Watch / debounce / throttle
* Dùng lại ở nhiều component

---

### 🧪 Test case BẮT BUỘC cho composable

#### 1. Initial state

```ts
it('init with correct default state', () => {})
```

#### 2. Happy path (logic chính)

```ts
it('handles success case correctly', () => {})
```

#### 3. Error path

```ts
it('handles error correctly', () => {})
```

#### 4. Side effect chính

```ts
expect(api.call).toHaveBeenCalled()
expect(router.push).toHaveBeenCalled()
```

> ❌ Không test edge case nhỏ, timing chi tiết, UI state phụ

---

## 3️⃣ Quy tắc cho `.vue` COMPONENT

### ❌ KHÔNG viết test nếu component:

* Chỉ nhận props → render UI
* Emit event đơn giản
* Không chứa logic nghiệp vụ
* Logic đã được tách xuống composable / store

---

### ✅ CHỈ viết test `.vue` khi:

1. **Component quyết định flow**

   * Page chính
   * Wizard
   * Form nhiều bước

2. **Có logic điều kiện quan trọng**

   * Permission
   * Feature flag
   * Role-based UI

3. **Có side effect trực tiếp**

   * Gọi API
   * Router navigation
   * Store mutation

👉 Test **hành vi**, không test layout.

---

## 4️⃣ QUY TRÌNH BẮT BUỘC: REFACTOR TRƯỚC KHI TEST

### ⚠️ Nếu `.vue` có UI phức tạp + logic lẫn nhau

#### KHÔNG viết test trực tiếp

👉 **PHẢI refactor theo bước sau:**

### 🔁 Bước 1: Tách logic ra composable

```ts
// useFormLogic.ts
export function useFormLogic() {
  // state
  // computed
  // validation
  // submit logic
}
```

### 🔁 Bước 2: Component chỉ còn UI

```vue
<script setup>
const {
  state,
  submit,
  error
} = useFormLogic()
</script>
```

### 🔁 Bước 3: Viết test cho composable

* Không viết test cho UI component
* Component được coi là “glue code”

---

## 5️⃣ Cách Gemini nên xử lý khi gặp UI phức tạp

### 👉 BẮT BUỘC làm theo thứ tự:

1. **Phân tích file `.vue`**
2. Nếu:

   * Logic > UI
   * Có nhiều `watch`, `computed`, `if/else`

   ➜ **Đề xuất refactor**
3. Tạo composable mới
4. Di chuyển logic
5. Viết test cho composable
6. Chỉ viết test `.vue` nếu component vẫn quyết định flow

---

## 6️⃣ Công cụ & chuẩn test

* Test runner: **Vitest**
* Mock bằng `vi.mock`
* Không snapshot test
* Không test CSS / DOM chi tiết

### 📁 Structure

```
composables/
  useX.ts
  __tests__/
    useX.spec.ts
```

---

## 7️⃣ Checklist trước khi viết test (Gemini PHẢI tự hỏi)

* [ ] Đây là logic hay chỉ là UI?
* [ ] Logic đã tách composable chưa?
* [ ] Test này có ngăn regression thật không?
* [ ] Có thể bỏ test `.vue` và chỉ test composable không?

Nếu câu trả lời là **YES** → **KHÔNG viết test component**

---

## 8️⃣ Câu chốt tiêu chuẩn

> “Nếu khó test → kiến trúc đang sai → refactor trước, test sau.”

---
