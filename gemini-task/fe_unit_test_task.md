# Frontend Unit Test Task - Hướng dẫn kiểm thử Pinia Stores với Service Dependencies

## Vấn đề

Trước đây, các unit test cho Pinia store thường gặp lỗi `TypeError: Cannot read properties of undefined` hoặc tương tự khi truy cập các service (ví dụ: `this.services.relationship`) trong các action của store. Điều này cho thấy các dependency service không được inject hoặc mock đúng cách trong môi trường test.

## Giải pháp và Các điểm quan trọng đã học từ `event.store.spec.ts`

Để đảm bảo các Pinia store được kiểm thử nhận được đối tượng `services` đã được mock đúng cách, chúng ta cần tuân thủ các nguyên tắc sau:

### 1. Mock toàn cục `createServices`

Sử dụng `vi.mock` để mock toàn bộ module `@/services/service.factory`. Hàm `createServices` được mock sẽ trả về một đối tượng chứa tất cả các service đã được mock. Các service không được sử dụng trực tiếp bởi store đang kiểm thử có thể được mock là các đối tượng rỗng.

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

### 2. Setup `beforeEach` nhất quán

Trong mỗi `beforeEach` block, hãy đảm bảo các bước sau được thực hiện:

*   **Khởi tạo Pinia:** `setActivePinia(createPinia());`
*   **Khởi tạo Store:** `store = useEventStore();` (thay `useEventStore` bằng store tương ứng).
*   **Reset Store State:** `store.$reset();` để đảm bảo trạng thái sạch cho mỗi test.
*   **Inject Service đã Mock:** `store.services = createServices('mock');` Đây là bước quan trọng để inject các service đã được mock toàn cục vào instance của store. Tham số `'mock'` đảm bảo rằng phiên bản mock của `createServices` được gọi.
*   **Reset Mock Functions:** `mockFetch.mockReset();` (và các mock function khác) để đảm bảo tính độc lập giữa các test.
*   **Thiết lập giá trị trả về mặc định cho Mock:** `mockDelete.mockResolvedValue(ok(undefined));` để tránh lỗi cho các hành động không phải là trọng tâm của test hiện tại. Các mock cho `_loadItems` nên được đặt trong từng `it` block nếu hành vi của nó thay đổi giữa các test.

```typescript
beforeEach(() => {
  const pinia = createPinia();
  setActivePinia(pinia);
  store = useEventStore(); // Thay thế bằng store tương ứng
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

### 3. Cấu trúc Test Actions

*   Mỗi action của store nên được kiểm thử trong một `describe` block riêng.
*   Sử dụng `it` block để mô tả các trường hợp test cụ thể (thành công, thất bại).
*   Sử dụng `mockResolvedValue` hoặc `mockRejectedValue` trên các mock function của service để mô phỏng phản hồi API.
*   Sử dụng `expect` để kiểm tra:
    *   Sự thay đổi trạng thái của store (`store.loading`, `store.error`, `store.items`).
    *   Giá trị trả về của action.
    *   Việc các mock function của service có được gọi đúng cách hay không (`toHaveBeenCalledTimes`, `toHaveBeenCalledWith`).

### 4. Xử lý lỗi

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

Bằng cách tuân thủ các nguyên tắc này, chúng ta có thể tạo ra các unit test mạnh mẽ và đáng tin cậy cho các Pinia store có dependency service, đảm bảo rằng các store hoạt động đúng như mong đợi trong mọi tình huống.