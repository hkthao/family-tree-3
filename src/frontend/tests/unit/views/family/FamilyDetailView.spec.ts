import { mount, flushPromises } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';
import { ref } from 'vue';
import FamilyDetailView from '@/views/family/FamilyDetailView.vue';
import { createTestingPinia } from '@pinia/testing';
import { useFamilyStore } from '@/stores/family.store';
import { FamilyVisibility, type Family } from '@/types';
import { createVuetify } from 'vuetify';
import { FamilyForm, TreeChart } from '@/components/family';
import { EventTimeline, EventCalendar } from '@/components/event';

const mockPush = vi.fn();
const route = ref({
  params: { id: '1' },
});

vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  useRoute: () => route.value,
}));

// Mock vue-i18n
vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: vi.fn((key) => key), // Mock t function to return the key
  }),
}));

vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key),
    },
  },
}));

// Mock child components
vi.mock('@/components/family', () => ({
  FamilyForm: {
    template: '<div class="mock-family-form"></div>',
    props: ['initialFamilyData', 'readOnly', 'title'],
  },
  TreeChart: {
    template: '<div class="mock-tree-chart"></div>',
    props: ['familyId'],
  },
}));

vi.mock('@/components/event', () => ({
  EventTimeline: {
    template: '<div class="mock-event-timeline"></div>',
    props: ['familyId', 'readOnly'],
  },
  EventCalendar: {
    template: '<div class="mock-event-calendar"></div>',
    props: ['familyId', 'readOnly'],
  },
}));

describe('FamilyDetailView.vue', () => {
  let familyStore: ReturnType<typeof useFamilyStore>;
  let vuetify: any;
  let pinia: ReturnType<typeof createTestingPinia>;

  const mockFamily: Family = {
    id: '1',
    name: 'Test Family',
    description: 'A family for testing',
    code: 'TF',
    visibility: FamilyVisibility.Public,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    pinia = createTestingPinia({
      createSpy: vi.fn,
    });
    familyStore = useFamilyStore(pinia);

    // Initialize Vuetify
    vuetify = createVuetify();

    // Reset mocks for vue-router
    mockPush.mockReset();
    route.value.params.id = mockFamily.id;

    // Mock familyStore actions
    (familyStore.getById as Mock).mockImplementation(async (id) => {
      if (id === mockFamily.id) {
        familyStore.currentItem = mockFamily;
        familyStore.error = null;
      } else {
        familyStore.currentItem = null;
        familyStore.error = 'Error';
      }
    });
  });

  it('should render family details when family data is available', async () => {
    // Mục tiêu của test: Đảm bảo rằng component hiển thị chi tiết thông tin gia đình
    // và các tab khác nhau (Timeline, Calendar, Tree) một cách chính xác khi có dữ liệu.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component với dữ liệu gia đình giả.
    const wrapper = mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();

    // Act & Assert: Kiểm tra thông tin chung và sự tồn tại của các component trong từng tab.
    // 1. Kiểm tra tab General
    expect(wrapper.text()).toContain(mockFamily.name);
    expect(wrapper.findComponent(FamilyForm).exists()).toBe(true);

    // 2. Chuyển và kiểm tra tab Timeline
    await wrapper.find('[data-testid="tab-timeline"]').trigger('click');
    await flushPromises();
    expect(wrapper.findComponent(EventTimeline).exists()).toBe(true);

    // 3. Chuyển và kiểm tra tab Calendar
    await wrapper.find('[data-testid="tab-calendar"]').trigger('click');
    await flushPromises();
    expect(wrapper.findComponent(EventCalendar).exists()).toBe(true);

    // 4. Chuyển và kiểm tra tab Family Tree
    await wrapper.find('[data-testid="tab-family-tree"]').trigger('click');
    await flushPromises();
    expect(wrapper.findComponent(TreeChart).exists()).toBe(true);

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Component cần hiển thị tên gia đình và form chi tiết ở tab mặc định.
    // - Khi người dùng chuyển tab, các component tương ứng (EventTimeline, EventCalendar, TreeChart)
    //   phải được render, chứng tỏ cơ chế tab hoạt động đúng.
  });

  it('should display "noData" alert when family data is not available and not loading', async () => {
    // Mục tiêu của test: Đảm bảo rằng cảnh báo "Không có dữ liệu" được hiển thị khi
    // không tìm thấy thông tin gia đình.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Thiết lập route với một ID không tồn tại để store trả về lỗi.
    route.value.params.id = '2'; // non-existent id
    const wrapper = mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Act: Chờ cho component được render sau khi fetch dữ liệu.
    await flushPromises();

    // Assert: Kiểm tra sự hiện diện của thông báo.
    expect(wrapper.text()).toContain('common.noData');

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Khi store không tìm thấy dữ liệu và không còn trong trạng thái loading,
    //   component phải hiển thị một thông báo rõ ràng cho người dùng biết rằng không có dữ liệu.
  });

  it('should display correct tab titles', async () => {
    // Mục tiêu của test: Đảm bảo rằng tiêu đề của các tab được hiển thị chính xác.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component.
    const wrapper = mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Act: Chờ cho component được render.
    await flushPromises();

    // Assert: Kiểm tra xem các key dịch của tiêu đề tab có trong nội dung của component không.
    expect(wrapper.text()).toContain('member.form.tab.general');
    expect(wrapper.text()).toContain('member.form.tab.timeline');
    expect(wrapper.text()).toContain('event.view.calendar');
    expect(wrapper.text()).toContain('family.tree.title');

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Giao diện người dùng phải hiển thị đúng các nhãn điều hướng để người dùng
    //   dễ dàng xác định các khu vực chức năng khác nhau của trang chi tiết.
  });

  it('should render FamilyForm in read-only mode', async () => {
    // Mục tiêu của test: Đảm bảo rằng FamilyForm được hiển thị ở chế độ chỉ đọc.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component.
    const wrapper = mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Act: Chờ component render và tìm component FamilyForm.
    await flushPromises();
    const familyForm = wrapper.findComponent(FamilyForm);

    // Assert: Kiểm tra prop 'readOnly' của FamilyForm.
    expect(familyForm.props().readOnly).toBe(true);

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Trang chi tiết mặc định là chế độ xem, không phải chỉnh sửa. Do đó, form
    //   phải ở chế độ chỉ đọc để ngăn người dùng thay đổi dữ liệu ngoài ý muốn.
  });

  it('should call familyStore.getById on onMounted', async () => {
    // Mục tiêu của test: Đảm bảo rằng action `getById` được gọi khi component được mounted.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component.
    mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Act: Chờ cho các tác vụ bất đồng bộ (timers) hoàn tất.
    await flushPromises();

    // Assert: Kiểm tra xem action `getById` đã được gọi với ID chính xác chưa.
    expect(familyStore.getById).toHaveBeenCalledWith(mockFamily.id);

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Component cần phải lấy dữ liệu chi tiết của gia đình ngay khi nó được tải
    //   để hiển thị thông tin cho người dùng.
  });

  it('should call familyStore.getById when route.params.id changes', async () => {
    // Mục tiêu của test: Đảm bảo rằng component fetch lại dữ liệu khi ID gia đình trên URL thay đổi.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component và kiểm tra lần gọi đầu tiên.
    const wrapper = mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();
    expect(familyStore.getById).toHaveBeenCalledWith(mockFamily.id);

    // Act: Thay đổi ID trên route và chờ cho watcher phản ứng.
    const newFamilyId = '2';
    route.value.params.id = newFamilyId;
    await wrapper.vm.$nextTick(); // Chờ cho watcher trigger
    await flushPromises();

    // Assert: Kiểm tra xem `getById` có được gọi lại với ID mới không.
    expect(familyStore.getById).toHaveBeenCalledWith(newFamilyId);

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Component phải có khả năng phản ứng với sự thay đổi của URL để cập nhật
    //   dữ liệu đang hiển thị, đảm bảo tính nhất quán khi người dùng điều hướng giữa các trang chi tiết.
  });

  it('should navigate to /family when "Close" button is clicked', async () => {
    // Mục tiêu của test: Đảm bảo nút "Đóng" điều hướng người dùng về trang danh sách gia đình.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component.
    const wrapper = mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();

    // Act: Tìm và click vào nút "Đóng".
    await wrapper.find('[data-testid="button-close"]').trigger('click');

    // Assert: Kiểm tra xem router.push có được gọi với đường dẫn chính xác không.
    expect(mockPush).toHaveBeenCalledWith('/family');

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Nút "Đóng" phải cung cấp một cách rõ ràng để người dùng thoát khỏi
    //   trang chi tiết và quay lại danh sách, cải thiện trải nghiệm người dùng.
  });

  it('should navigate to /family/edit/:id when "Edit" button is clicked', async () => {
    // Mục tiêu của test: Đảm bảo nút "Sửa" điều hướng người dùng đến trang chỉnh sửa.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component.
    const wrapper = mount(FamilyDetailView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();

    // Act: Tìm và click vào nút "Sửa".
    await wrapper.find('[data-testid="button-edit"]').trigger('click');

    // Assert: Kiểm tra xem router.push có được gọi với URL chỉnh sửa chính xác không.
    expect(mockPush).toHaveBeenCalledWith(`/family/edit/${mockFamily.id}`);

    // Giải thích vì sao kết quả mong đợi là đúng:
    // - Nút "Sửa" phải cho phép người dùng chuyển từ chế độ xem sang chế độ chỉnh sửa
    //   một cách thuận tiện, là một luồng công việc cơ bản của ứng dụng CRUD.
  });
});
