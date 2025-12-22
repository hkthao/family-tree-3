import { reactive, computed, watch, toRef } from 'vue';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables';
import type { Paginated, ListOptions } from '@/types/pagination';
import type { MediaItem, FamilyMedia } from '@/types';
import { MediaType } from '@/types/enums'; // Import MediaType
import type { FamilyMediaFilter } from '@/types/familyMedia';
import type { IFamilyMediaService } from '@/services/family-media/family-media.service.interface';
import { getMediaTypeEnumValue } from './familyMedia.utils'; // Import from utils

export interface MediaListOptions extends ListOptions {
  familyId?: string;
  searchQuery?: string;
  mediaType?: MediaType | string;
}

interface UseMediaPickerDataDeps {
  useQuery: typeof useQuery;
  useQueryClient: typeof useQueryClient;
  useServices: typeof useServices;
}

const defaultDeps: UseMediaPickerDataDeps = {
  useQuery,
  useQueryClient,
  useServices,
};

function mapFamilyMediaToMediaItem(familyMedia: FamilyMedia): MediaItem {
  return {
    id: familyMedia.id,
    url: familyMedia.filePath,
    type: familyMedia.mediaType.toString(),
  };
}

export function useMediaPickerData(
  options: MediaListOptions,
  deps: UseMediaPickerDataDeps = defaultDeps,
) {
  const { useQuery: injectedUseQuery, useServices: injectedUseServices } = deps;
  const { familyMedia: familyMediaService } = injectedUseServices();

  const reactiveOptions = reactive<MediaListOptions>({
    page: options.page || 1,
    itemsPerPage: options.itemsPerPage || 10,
    sortBy: options.sortBy || [],
    familyId: options.familyId,
    searchQuery: options.searchQuery || '',
    mediaType: options.mediaType || '',
  });

  // Convert reactiveOptions.familyId to a Ref so useQuery can watch it
  const familyIdRef = toRef(reactiveOptions, 'familyId');

  const queryResult = injectedUseQuery<Paginated<MediaItem>, Error>({
    queryKey: [
      'family-media-picker',
      reactiveOptions.familyId,
      reactiveOptions.searchQuery,
      reactiveOptions.mediaType,
      reactiveOptions.page,
      reactiveOptions.itemsPerPage,
      reactiveOptions.sortBy,
    ],
    queryFn: async () => {
      if (!familyIdRef.value) {
        return { items: [], totalItems: 0, page: 1, itemsPerPage: 10, totalPages: 0 };
      }

      const { searchQuery, mediaType, familyId, ...listOptions } = reactiveOptions; // Destructure familyId out for filters
      const filters: FamilyMediaFilter = {
        searchQuery: searchQuery || undefined,
        mediaType: getMediaTypeEnumValue(mediaType),
        familyId: familyId, // familyId is now part of filters
      };

      const result = await (familyMediaService as IFamilyMediaService).search(listOptions, filters);
      if (result.ok) {
        const mappedItems = result.value.items.map(mapFamilyMediaToMediaItem);
        return {
          ...result.value,
          items: mappedItems,
        };
      }
      throw result.error;
    },
    enabled: computed(() => !!familyIdRef.value), // Only run query if familyId is available
  });

  // Watch for errors and log them
  watch(queryResult.error, (newError) => {
    if (newError) {
      console.error('useQuery queryFn error:', newError);
    }
  });

  const mediaList = computed(() => queryResult.data.value?.items || []);
  const totalItems = computed(() => queryResult.data.value?.totalItems || 0);
  const isLoading = computed(() => queryResult.isFetching.value);

  const setPage = (page: number) => { reactiveOptions.page = page; };
  const setItemsPerPage = (itemsPerPage: number) => { reactiveOptions.itemsPerPage = itemsPerPage; };
  const setSortBy = (sortBy: ListOptions['sortBy']) => { reactiveOptions.sortBy = sortBy; };
  const setSearchQuery = (query: string) => { reactiveOptions.searchQuery = query; };
  const setMediaType = (mediaType: string | MediaType | undefined) => { reactiveOptions.mediaType = mediaType; };
  const setFamilyId = (familyId: string) => { reactiveOptions.familyId = familyId; };

  const handleOptionsChange = (newOptions: MediaListOptions) => {
    reactiveOptions.familyId = newOptions.familyId;
    reactiveOptions.searchQuery = newOptions.searchQuery || '';
    reactiveOptions.mediaType = newOptions.mediaType || '';
    reactiveOptions.page = newOptions.page || 1;
    reactiveOptions.itemsPerPage = newOptions.itemsPerPage || 10;
    reactiveOptions.sortBy = newOptions.sortBy || [];
  };

  watch(() => options, handleOptionsChange, { deep: true });

  return {
    state: {
      mediaList,
      totalItems,
      isLoading,
      error: queryResult.error,
      pagination: computed(() => ({
        page: reactiveOptions.page,
        itemsPerPage: reactiveOptions.itemsPerPage,
        sortBy: reactiveOptions.sortBy,
      })),
      filters: computed(() => ({
        familyId: reactiveOptions.familyId,
        searchQuery: reactiveOptions.searchQuery,
        mediaType: reactiveOptions.mediaType,
      })),
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
      setSearchQuery,
      setMediaType,
      setFamilyId,
      refetch: queryResult.refetch,
    },
  };
}