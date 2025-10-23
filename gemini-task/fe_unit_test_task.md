# Frontend Unit Test Task - Hướng dẫn kiểm thử Pinia Stores và Vue Components với Service Dependencies

## Vấn đề

Trước đây, các unit test cho Pinia store và Vue components thường gặp lỗi `TypeError: Cannot read properties of undefined` hoặc tương tự khi truy cập các service (ví dụ: `this.services.relationship`) trong các action của store, hoặc khi tương tác với các component con và thư viện UI (như Vuetify). Điều này cho thấy các dependency service, Pinia store instances, hoặc các component/thư viện UI không được inject hoặc mock đúng cách trong môi trường test.

## Giải pháp và Các điểm quan trọng đã học

Để đảm bảo các Pinia store và Vue components được kiểm thử nhận được các dependency đã được mock đúng cách, chúng ta cần tuân thủ các nguyên tắc sau:

### 1. Mock toàn cục `createServices`

Sử dụng `vi.mock` để mock toàn bộ module `@/services/service.factory`. Hàm `createServices` được mock sẽ trả về một đối tượng chứa tất cả các service đã được mock. Các service không được sử dụng trực tiếp bởi store hoặc component đang kiểm thử có thể được mock là các đối tượng rỗng.

```typescript
// Mock the IEventService (hoặc bất kỳ service nào khác)
const mockFetch = vi.fn();
const mockGetById = vi.fn();
// ... các mock function khác cho service cụ thể

// Mock toàn bộ service factory để kiểm soát việc inject service
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    event: { // Tên service tương ứng với store đang test
      fetch: mockFetch,
      getById: mockGetById,
      // ... các mock function đã định nghĩa
    },
    // Thêm các service khác dưới dạng đối tượng rỗng nếu chúng không được sử dụng trực tiếp
    ai: {},
    auth: {},
    // ... các service khác
  })),
}));
```

### 2. Mock `vue-router` và `vue-i18n`

Khi component hoặc store sử dụng `useRouter` hoặc `useI18n`, cần mock chúng để kiểm soát hành vi điều hướng và dịch thuật.

```typescript
// Mock vue-router
const mockPush = vi.fn();
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock vue-i18n (cho component sử dụng useI18n)
vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: vi.fn((key) => key), // Mock t function to return the key
  }),
}));

// Mock @/plugins/i18n (cho store hoặc nơi khác sử dụng i18n.global.t)
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key),
    },
  },
}));
```

### 3. Setup `beforeEach` nhất quán cho Store Tests

Trong mỗi `beforeEach` block của store tests, hãy đảm bảo các bước sau được thực hiện:

*   **Khởi tạo Pinia:** `const pinia = createTestingPinia({ createSpy: vi.fn });`
*   **Khởi tạo Store:** `store = useEventStore(pinia);` (thay `useEventStore` bằng store tương ứng và **truyền instance `pinia` vào**).
*   **Reset Store State:** `store.$reset();` để đảm bảo trạng thái sạch cho mỗi test.
*   **Inject Service đã Mock:** `store.services = createServices('mock');` Đây là bước quan trọng để inject các service đã được mock toàn cục vào instance của store. Tham số `'mock'` đảm bảo rằng phiên bản mock của `createServices` được gọi.
*   **Reset Mock Functions:** `mockFetch.mockReset();` (và các mock function khác) để đảm bảo tính độc lập giữa các test.
*   **Thiết lập giá trị trả về mặc định cho Mock:** `mockDelete.mockResolvedValue(ok(undefined));` để tránh lỗi cho các hành động không phải là trọng tâm của test hiện tại. Các mock cho `_loadItems` nên được đặt trong từng `it` block nếu hành vi của nó thay đổi giữa các test.

```typescript
beforeEach(() => {
  vi.clearAllMocks();
  const pinia = createTestingPinia({
    createSpy: vi.fn,
  });
  store = useEventStore(pinia); // Truyền pinia instance
  store.$reset();
  // Inject thủ công các service đã được mock
  // @ts-ignore
  store.services = createServices('mock');
  // Reset các mock trước mỗi test
  mockFetch.mockReset();
  mockGetById.mockReset();
  // ... reset các mock function khác
  mockDelete.mockResolvedValue(ok(undefined));
  mockAddItems.mockResolvedValue(ok(['new-id-1']));
  // mockLoadItems.mockResolvedValue(ok(mockPaginatedEvents)); // Đặt trong từng it block nếu cần
});
```

### 4. Setup `beforeEach` nhất quán cho Component Tests

Khi test Vue components sử dụng Pinia stores và Vuetify, cần cấu hình `mount` options một cách cẩn thận:

*   **Khởi tạo Pinia và Vuetify:** Tạo instance `pinia` với `createTestingPinia({ createSpy: vi.fn })` và `vuetify` với `createVuetify()`. Lưu ý rằng `createTestingPinia` cần được gọi trực tiếp trong `plugins` array của `mount` để đảm bảo Pinia được khởi tạo đúng cách cho mỗi test.
*   **Truyền vào `global.plugins`:** Đảm bảo cả `createTestingPinia({ createSpy: vi.fn })` và `vuetify` được truyền vào mảng `plugins` trong `global` options của `mount`.

```typescript
  let familyStore: ReturnType<typeof useFamilyStore>;
  let notificationStore: ReturnType<typeof useNotificationStore>;
  let vuetify: any;

  beforeEach(() => {
    vi.clearAllMocks();
    // Khởi tạo Pinia và các store
    const pinia = createTestingPinia({
      createSpy: vi.fn,
    });
    familyStore = useFamilyStore(pinia); // Truyền pinia instance
    notificationStore = useNotificationStore(pinia); // Truyền pinia instance

    // Khởi tạo Vuetify
    vuetify = createVuetify();
  });

  it('should render correctly', () => {
    const wrapper = mount(FamilyAddView, {
      global: {
        plugins: [createTestingPinia({ createSpy: vi.fn }), vuetify], // Truyền cả pinia và vuetify
      },
    });
    // ... assertions
  });
```

### 5. Mocking Component Refs và Methods

Khi một component cha tương tác với các method của component con thông qua `ref` (ví dụ: `familyFormRef.value.validate()`), cần mock các method này trên instance của component con. Điều này được thực hiện bằng cách gán trực tiếp các `vi.fn()` đã mock vào các thuộc tính tương ứng của `familyForm.vm`.

```typescript
    const familyForm = wrapper.findComponent(FamilyForm);
    // Gán mock component instance vào ref của component cha
    (wrapper.vm as any).familyFormRef = familyForm.vm;

    // Mock các method của component con
    (familyForm.vm as any).validate = vi.fn().mockResolvedValue(true);
    (familyForm.vm as any).getFormData = vi.fn().mockReturnValue({
      name: 'Test Family',
      description: 'A family for testing',
    });
```

### 6. Cấu trúc Test Actions

*   Mỗi action của store hoặc hành vi của component nên được kiểm thử trong một `describe` block riêng.
*   Sử dụng `it` block để mô tả các trường hợp test cụ thể (thành công, thất bại, validation).
*   Sử dụng `mockResolvedValue` hoặc `mockRejectedValue` trên các mock function của service hoặc component con để mô phỏng phản hồi.
*   Sử dụng `expect` để kiểm tra:
    *   Sự thay đổi trạng thái của store (`store.loading`, `store.error`, `store.items`).
    *   Giá trị trả về của action.
    *   Việc các mock function của service hoặc component con có được gọi đúng cách hay không (`toHaveBeenCalledTimes`, `toHaveBeenCalledWith`).

### 7. Xử lý lỗi

*   Luôn bao gồm các test case xử lý lỗi, kiểm tra rằng `store.error` được thiết lập và `store.loading` được reset về `false` khi có lỗi từ service.

```typescript
it('should handle load items failure', async () => {
  const errorMessage = 'Failed to load events.';
  mockLoadItems.mockResolvedValue(err({ message: errorMessage } as ApiError));

  await store._loadItems();

  expect(store.loading).toBe(false);
  expect(store.error).toBeTruthy();
  expect(store.items).toEqual([]);
  expect(mockLoadItems).toHaveBeenCalledTimes(1);
});
```

Bằng cách tuân thủ các nguyên tắc này, chúng ta có thể tạo ra các unit test mạnh mẽ và đáng tin cậy cho các Pinia store và Vue components có dependency service, đảm bảo rằng các store và component hoạt động đúng như mong đợi trong mọi tình huống.