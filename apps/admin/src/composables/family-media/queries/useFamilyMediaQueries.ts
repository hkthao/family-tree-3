import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyMedia, FamilyMediaFilter, Paginated, ListOptions } from '@/types';
import { ApiFamilyMediaService } from '@/services/family-media/api.family-media.service';
import type { IFamilyMediaService } from '@/services/family-media/family-media.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyMediaService: IFamilyMediaService = new ApiFamilyMediaService(apiClient);

/**
 * Composible để lấy danh sách Family Media có phân trang.
 * @param filters Ref<FamilyMediaFilter> - Các bộ lọc cho danh sách media.
 * @param listOptions Ref<ListOptions> - Các tùy chọn phân trang và sắp xếp.
 */
export function useFamilyMediaListQuery(filters: Ref<FamilyMediaFilter>, listOptions: Ref<ListOptions>) {

  const query = useQuery<Paginated<FamilyMedia>, Error>({
    queryKey: computed(() => (unref(listOptions).itemsPerPage !== -1 ? queryKeys.familyMedia.list(
      unref(filters),
      unref(listOptions).page,
      unref(listOptions).itemsPerPage,
      unref(listOptions).sortBy
    ) : [])),
    queryFn: async () => {
      const currentFilters = unref(filters);
      const currentListOptions = unref(listOptions);
      const response = await apiFamilyMediaService.search(
        currentFilters.familyId!,
        currentFilters,
        currentListOptions.page,
        currentListOptions.itemsPerPage,
        currentListOptions.sortBy,
      );
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    enabled: computed(() => unref(listOptions).itemsPerPage !== -1),
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
export function useFamilyMediaQuery(familyMediaId: Ref<string | undefined>) {
  const query = useQuery<FamilyMedia, Error>({
    queryKey: computed(() => (unref(familyMediaId) ? queryKeys.familyMedia.detail(unref(familyMediaId)!) : [])),
    queryFn: async () => {
      const response = await apiFamilyMediaService.getById(familyMediaId.value!);
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
