import { useQuery } from '@tanstack/vue-query';
import { computed, type Ref } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import { type Paginated, type ListOptions, type FilterOptions, type ImageRestorationJobDto } from '@/types';

export const useImageRestorationJobsQuery = (
  familyId: Ref<string>,
  options: Ref<ListOptions>,
  filters: Ref<FilterOptions>,
) => {
  const services = useServices();
  const query = useQuery<Paginated<ImageRestorationJobDto> | undefined, Error>({ // Explicitly type query.data
    queryKey: ['image-restoration-jobs', familyId, options, filters],
    queryFn: async () => {
      if (!familyId.value) {
        return { items: [], page: 1, totalItems: 0, totalPages: 0 }; // Return empty paginated object
      }
      const allFilters = { ...filters.value, familyId: familyId.value }; // Add familyId to filters
      const result = await services.imageRestorationJob.search(options.value, allFilters);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    enabled: computed(() => !!familyId.value),
  });

  return {
    state: {
      imageRestorationJobs: computed(() => query.data.value?.items || []),
      totalItems: computed(() => query.data.value?.totalItems || 0),
      isLoading: query.isLoading,
      error: query.error,
    },
  };
};