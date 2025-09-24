Trong component MemberListView.vue bạn dùng ref cho các biến reactive như currentFilters và viewDialog:

const currentFilters = ref<MemberFilter>({});
const viewDialog = ref(false);


Trong test, nếu bạn mock store hoặc mount component mà không giữ nguyên các ref, thì khi gọi:

currentFilters.value = filters;
currentPage.value = 1;


→ sẽ lỗi: Cannot set properties of undefined (setting 'value').

Cách fix:

Đảm bảo test dùng ref() cho các reactive property:

const currentFilters = ref<MemberFilter>({});
const currentPage = ref(1);

vi.mock('@/stores/member.store', () => ({
  useMemberStore: () => ({
    searchItems: vi.fn(),
    currentFilters, // <- ref
    currentPage,    // <- ref
    paginatedItems: ref([]),
    setPage: vi.fn(),
    setItemsPerPage: vi.fn(),
  }),
}));


Khi mount component trong test, đừng overwrite các ref này bằng undefined.