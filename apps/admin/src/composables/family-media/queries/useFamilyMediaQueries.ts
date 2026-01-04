import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyMedia, FamilyMediaFilter, Paginated, ListOptions } from '@/types';
import type { IFamilyMediaService } from '@/services/family-media/family-media.service.interface';
import { useServices } from '@/plugins/services.plugin';
import { queryKeys } from '@/constants/queryKeys'; // Added missing import


/**
 * Composible để lấy danh sách Family Media có phân trang.
 * @param filters Ref<FamilyMediaFilter> - Các bộ lọc cho danh sách media.
 * @param page Ref<number> - Trang hiện tại.
 * @param itemsPerPage Ref<number> - Số mục trên mỗi trang.
 * @param sortBy Ref<ListOptions['sortBy']> - Tùy chọn sắp xếp.
 */
export function useFamilyMediaListQuery(
  filters: Ref<FamilyMediaFilter>,
  page: Ref<number>,
  itemsPerPage: Ref<number>,
  sortBy: Ref<ListOptions['sortBy']>,
  service: IFamilyMediaService = useServices().familyMedia, // Inject service
) {
  const query = useQuery<Paginated<FamilyMedia>, Error>({
    queryKey: computed(() => {
      if (unref(itemsPerPage) === -1) return [];
      const listOptions: ListOptions = {
        page: unref(page),
        itemsPerPage: unref(itemsPerPage),
        sortBy: unref(sortBy),
      };
      return queryKeys.familyMedia.list(listOptions, unref(filters));
    }),
    queryFn: async () => {
      const currentFilters = unref(filters);
      const currentPage = unref(page);
      const currentItemsPerPage = unref(itemsPerPage);
      const currentSortBy = unref(sortBy);

      const listOptions: ListOptions = {
        page: currentPage,
        itemsPerPage: currentItemsPerPage,
        sortBy: currentSortBy,
      };
      const response = await service.search(listOptions, currentFilters);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    enabled: computed(() => unref(itemsPerPage) !== -1),
    staleTime: 1000 * 60, // 1 minute
  });

  const familyMediaList = computed(() => query.data.value?.items || []);
  const totalItems = computed(() => query.data.value?.totalItems || 0);
  const isLoading = computed(() => query.isFetching.value);

  return {
    query,
    familyMediaList,
    totalItems,
    isLoading,
    error: query.error,
    refetch: query.refetch,
  };
}

/**
 * Composible để lấy thông tin chi tiết một Family Media theo ID.
 * @param familyId Ref<string | undefined> - ID của gia đình.
 * @param familyMediaId Ref<string | undefined> - ID của media item.
 */
export function useFamilyMediaQuery(
  familyMediaId: Ref<string | undefined>,
  service: IFamilyMediaService = useServices().familyMedia, // Inject service
) {
  const query = useQuery<FamilyMedia, Error>({
    queryKey: computed(() => (unref(familyMediaId) ? queryKeys.familyMedia.detail(unref(familyMediaId)!) : [])),
    queryFn: async () => {
      const response = await service.getById(familyMediaId.value!);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    enabled: computed(() => !!unref(familyMediaId)),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const familyMedia = computed(() => query.data.value);
  const isLoading = computed(() => query.isFetching.value);

  return {
    query,
    familyMedia,
    isLoading,
    error: query.error,
    refetch: query.refetch,
  };
}