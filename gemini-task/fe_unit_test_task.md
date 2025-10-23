# Frontend Unit Test Task - Hướng dẫn kiểm thử Pinia Stores và Vue Components

## Vấn đề

Unit test cho Pinia store và Vue component thường gặp lỗi khi tương tác với các dependency như services, store instances khác, hoặc các thư viện UI (Vuetify). Lỗi thường là `TypeError: Cannot read properties of undefined` hoặc component không render đúng như mong đợi. Điều này xảy ra do các dependency không được inject hoặc mock đúng cách trong môi trường test.

## Giải pháp và Kinh nghiệm

### 1. Mocking Dependencies

#### 1.1. Mock Services

Sử dụng `vi.mock` để mock toàn bộ module `@/services/service.factory`. Hàm `createServices` được mock sẽ trả về một đối tượng chứa các service đã được mock.

```typescript
// Mock các hàm của service
const mockFetch = vi.fn();
const mockGetById = vi.fn();

// Mock service factory
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    event: { // Tên service
      fetch: mockFetch,
      getById: mockGetById,
    },
    ai: {}, // Mock rỗng cho các service không dùng
  })),
}));
```

#### 1.2. Mock `vue-router`

- **Mock cơ bản:** Nếu chỉ cần mock `useRouter` để kiểm tra điều hướng (`push`).

```typescript
const mockPush = vi.fn();
vi.mock('vue-router', () => ({
  useRouter: () => ({ push: mockPush }),
  useRoute: () => ({ params: {} }), // Cung cấp route mặc định
}));
```

- **Mock `useRoute` với `ref` cho Watchers:** Khi component theo dõi (`watch`) sự thay đổi của `route.params`, `useRoute` phải trả về một đối tượng reactive. Sử dụng `ref` từ `vue` để tạo mock này.

```typescript
import { ref } from 'vue';

const route = ref({ params: { id: '1' } });

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: mockPush }),
  useRoute: () => route.value,
}));

// Trong test, thay đổi route bằng cách thay đổi .value
route.value.params.id = '2';
await wrapper.vm.$nextTick(); // Chờ watcher cập nhật
```

#### 1.3. Mock `vue-i18n`

Mock `useI18n` và plugin `i18n` để kiểm soát việc dịch thuật.

```typescript
vi.mock('vue-i18n', () => ({
  useI18n: () => ({ t: (key) => key }),
}));

vi.mock('@/plugins/i18n', () => ({
  default: { global: { t: (key) => key } },
}));
```

#### 1.4. Mock API trình duyệt và Thư viện bên thứ ba

JSDOM (môi trường test của Vitest) không có sẵn một số API của trình duyệt như `ResizeObserver` (được Vuetify sử dụng). Cần mock chúng trong file setup test (`tests/setup.ts`).

```typescript
// tests/setup.ts
const ResizeObserverMock = vi.fn(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));

vi.stubGlobal('ResizeObserver', ResizeObserverMock);
```

### 2. Setup Test

#### 2.1. `beforeEach` cho Store Tests

- **Khởi tạo Pinia và Store:** Luôn truyền `pinia` instance vào `useStore`.
- **Inject Services:** Gán các service đã mock vào `store.services`.

```typescript
beforeEach(() => {
  vi.clearAllMocks();
  const pinia = createTestingPinia({ createSpy: vi.fn });
  store = useEventStore(pinia); // Truyền pinia
  store.$reset();
  // @ts-ignore
  store.services = createServices('mock');
  mockFetch.mockReset();
});
```

#### 2.2. `beforeEach` cho Component Tests

- **Chia sẻ `pinia` instance:** Khai báo `pinia` ở scope của `describe` và khởi tạo nó trong `beforeEach`. Sau đó, truyền cùng một `pinia` instance này vào `plugins` của `mount`.
- **Lỗi thường gặp:** Nếu `mount` sử dụng `createTestingPinia` mới, component sẽ có một store khác với store trong test, dẫn đến mock không hoạt động.

```typescript
describe('MyComponent.vue', () => {
  let pinia: ReturnType<typeof createTestingPinia>;
  let vuetify: any;

  beforeEach(() => {
    pinia = createTestingPinia({ createSpy: vi.fn });
    vuetify = createVuetify();
    // ... mock stores
  });

  it('should work', () => {
    const wrapper = mount(MyComponent, {
      global: {
        plugins: [pinia, vuetify], // Dùng chung instance
      },
    });
  });
});
```

### 3. Viết Test cho Component

#### 3.1. Xử lý Tác vụ Bất đồng bộ

- **Sử dụng `flushPromises`:** Khi test các hành động bất đồng bộ (ví dụ: gọi API trong `onMounted`), sử dụng `await flushPromises()` từ `@vue/test-utils` để đảm bảo tất cả các promise đã được giải quyết trước khi thực hiện assertion.
- **Tránh `vi.runOnlyPendingTimers()`:** Hàm này chỉ dành cho timers (`setTimeout`, `setInterval`), không dùng cho promise.

```typescript
it('should render data after mount', async () => {
  const wrapper = mount(MyComponent, { /* ... */ });
  await flushPromises(); // Chờ onMounted và các promise khác hoàn tất
  expect(wrapper.text()).toContain('dữ liệu đã render');
});
```

#### 3.2. Lựa chọn Element để Tương tác

- **Ưu tiên `data-testid`:** Thay vì dùng class CSS hoặc cấu trúc DOM dễ thay đổi, hãy thêm thuộc tính `data-testid` vào các element quan trọng trong component.
- **Cách dùng:**
  - Trong component: `<v-btn data-testid="button-submit">Lưu</v-btn>`
  - Trong test: `await wrapper.find('[data-testid="button-submit"]').trigger('click');`

#### 3.3. Cấu trúc Test Method rõ ràng

Sử dụng comment để làm rõ mục tiêu, các bước, và lý do cho kết quả mong đợi.

```typescript
it('should navigate to edit page when Edit button is clicked', async () => {
  // Mục tiêu: Đảm bảo nút "Sửa" điều hướng đúng trang.
  
  // Arrange: Khởi tạo component.
  const wrapper = mount(MyComponent, { /* ... */ });
  await flushPromises();

  // Act: Tìm và click nút "Sửa".
  await wrapper.find('[data-testid="button-edit"]').trigger('click');

  // Assert: Kiểm tra router.push được gọi đúng.
  expect(mockPush).toHaveBeenCalledWith('/edit/1');

  // Giải thích: Nút "Sửa" là một phần quan trọng của luồng CRUD.
});
```

Bằng cách tuân thủ các nguyên tắc này, unit test sẽ trở nên mạnh mẽ, dễ bảo trì và ít bị ảnh hưởng bởi các thay đổi về cấu trúc UI.