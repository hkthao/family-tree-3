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
 * @param familyId Ref<string | undefined> - ID của gia đình.
 * @param filters Ref<FamilyMediaFilter> - Các bộ lọc cho danh sách media.
 * @param listOptions Ref<ListOptions> - Các tùy chọn phân trang và sắp xếp.
 */
export function useFamilyMediaListQuery(familyId: Ref<string | undefined>, filters: Ref<FamilyMediaFilter>, listOptions: Ref<ListOptions>) {
  const query = useQuery<Paginated<FamilyMedia>, Error>({
    queryKey: computed(() => (unref(familyId) ? queryKeys.familyMedia.list(
      unref(familyId)!,
      unref(filters),
      unref(listOptions).page,
      unref(listOptions).itemsPerPage,
      unref(listOptions).sortBy
    ) : [])),
    queryFn: async () => {
      const id = unref(familyId);
      const currentFilters = unref(filters);
      const currentListOptions = unref(listOptions);
      if (!id) {
        throw new Error('Family ID is required for fetching family media list');
      }
      const response = await apiFamilyMediaService.search(
        id,
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
    enabled: computed(() => !!unref(familyId)),
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
export function useFamilyMediaQuery(familyId: Ref<string | undefined>, familyMediaId: Ref<string | undefined>) {
  const query = useQuery<FamilyMedia, Error>({
    queryKey: computed(() => (unref(familyId) && unref(familyMediaId) ? queryKeys.familyMedia.detail(unref(familyId)!, unref(familyMediaId)!) : [])),
    queryFn: async () => {
      const fId = unref(familyId);
      const fmId = unref(familyMediaId);
      if (!fId || !fmId) {
        throw new Error('Family ID and Family Media ID are required');
      }
      const response = await apiFamilyMediaService.getById(fmId);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    enabled: computed(() => !!unref(familyId) && !!unref(familyMediaId)),
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
