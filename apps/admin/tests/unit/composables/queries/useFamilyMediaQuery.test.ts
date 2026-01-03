// apps/admin/tests/unit/composables/queries/useFamilyMediaQuery.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref, computed } from 'vue';
import { useFamilyMediaQuery } from '@/composables/queries/useFamilyMediaQuery';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyMedia, Paginated, ListOptions, FamilyMediaFilter } from '@/types';
import { MediaType } from '@/types/enums';

// Mock @tanstack/vue-query's useQuery
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
}));

// Mock @/plugins/services.plugin
const mockFamilyMediaService = {
  search: vi.fn(),
};
vi.mock('@/plugins/services.plugin', () => ({
  useServices: () => ({
    familyMedia: mockFamilyMediaService,
  }),
}));

// Mock queryKeys (though useQuery mock might make this less critical, good practice)
vi.mock('@/constants/queryKeys', () => ({
  queryKeys: {
    familyMedia: {
      all: ['familyMedia'] as const,
      list: vi.fn((options, filters) => ['familyMedia', 'list', options, filters]),
    },
  },
}));

describe('useFamilyMediaQuery', () => {
  const mockListOptionsRef = ref<ListOptions>({ page: 1, itemsPerPage: 10 });
  const mockFiltersRef = ref<FamilyMediaFilter>({ familyId: 'family123', mediaType: MediaType.Image });

  const mockListOptions = computed(() => mockListOptionsRef.value);
  const mockFilters = computed(() => mockFiltersRef.value);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should call useQuery with correct parameters', () => {
    // Mock the return value of useQuery
    vi.mocked(useQuery).mockReturnValue(
      computed(() => ({
        data: ref({ items: [], totalItems: 0 }),
        isPending: ref(false),
        isFetching: ref(false),
        error: ref(null),
      })) as any
    );

    useFamilyMediaQuery(mockListOptions, mockFilters);

    expect(useQuery).toHaveBeenCalledOnce();
    const [optionsArg] = vi.mocked(useQuery).mock.calls[0];
    const options = optionsArg as any; // Cast to any to resolve typing issue

    expect(options.queryKey.value).toEqual(['familyMedia', 'list', mockListOptions.value, mockFilters.value]);
    expect(options.staleTime).toBe(1000 * 60 * 5); // 5 minutes
    expect(typeof options.queryFn).toBe('function');
  });

  it('should return family media data, total items, loading and error states', async () => {
    const mockData: Paginated<FamilyMedia> = {
      items: [
        { id: '1', fileName: 'test1.jpg', filePath: '/path/to/test1.jpg', mediaType: MediaType.Image, familyId: 'family123', fileSize: 1000, created: new Date() }, // Changed to new Date()
      ],
      totalItems: 1,
      page: 1, // Added
      totalPages: 1, // Added
    };

    // Mock useQuery to return data
    vi.mocked(useQuery).mockReturnValue(
      computed(() => ({
        data: ref(mockData),
        isPending: ref(false),
        isFetching: ref(false),
        error: ref(null),
      })) as any
    );

    const { familyMedia, totalItems, queryLoading, queryError } = useFamilyMediaQuery(mockListOptions, mockFilters);

    expect(familyMedia.value).toEqual(mockData.items);
    expect(totalItems.value).toBe(mockData.totalItems);
    expect(queryLoading.value).toBe(false);
    expect(queryError.value).toBeNull();
  });

  it('should set queryLoading to true when fetching', () => {
    vi.mocked(useQuery).mockReturnValue(
      computed(() => ({
        data: ref(null),
        isPending: ref(true),
        isFetching: ref(true),
        error: ref(null),
      })) as any
    );

    const { queryLoading } = useFamilyMediaQuery(mockListOptions, mockFilters);
    expect(queryLoading.value).toBe(true);
  });

  it('should set queryError when fetching fails', async () => {
    const errorMessage = 'Network error';
    vi.mocked(useQuery).mockReturnValue(
      computed(() => ({
        data: ref(null),
        isPending: ref(false),
        isFetching: ref(false),
        error: ref(new Error(errorMessage)),
      })) as any
    );

    const { queryError } = useFamilyMediaQuery(mockListOptions, mockFilters);
    expect(queryError.value).toBeInstanceOf(Error);
    expect(queryError.value?.message).toBe(errorMessage);
  });

  it('should call familyMediaService.search in queryFn', async () => {
    const mockData: Paginated<FamilyMedia> = {
      items: [],
      totalItems: 0,
      page: 1, // Added
      totalPages: 1, // Added
    };
    let queryFn: Function | undefined;
    vi.mocked(useQuery).mockImplementation((options) => {
      queryFn = (options as any).queryFn as Function;
      return computed(() => ({
        data: ref(null),
        isPending: ref(false),
        isFetching: ref(false),
        error: ref(null),
      })) as any;
    });

    useFamilyMediaQuery(mockListOptions, mockFilters);

    // Manually call the queryFn that useQuery would invoke
    if (queryFn) {
      const result = await queryFn();
      expect(mockFamilyMediaService.search).toHaveBeenCalledWith(mockListOptions.value, mockFilters.value);
      expect(result).toEqual(mockData);
    } else {
      expect.fail('queryFn was not set');
    }
  });

  it('should throw an error if familyMediaService.search fails', async () => {
    const errorMessage = 'Failed to fetch media from service';
    mockFamilyMediaService.search.mockResolvedValueOnce({ ok: false, error: { message: errorMessage, code: 500 } });

    let queryFn: Function | undefined;
    vi.mocked(useQuery).mockImplementation((options) => {
      queryFn = (options as any).queryFn as Function;
      return computed(() => ({
        data: ref(null),
        isPending: ref(false),
        isFetching: ref(false),
        error: ref(null),
      })) as any;
    });

    useFamilyMediaQuery(mockListOptions, mockFilters);

    if (queryFn) {
      await expect(queryFn()).rejects.toThrow(errorMessage);
    } else {
      expect.fail('queryFn was not set');
    }
  });
});
