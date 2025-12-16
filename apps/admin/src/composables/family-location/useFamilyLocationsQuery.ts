import { useQuery } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyLocationFilter, ListOptions, FilterOptions } from '@/types';
import { unref } from 'vue';

export const useFamilyLocationsQuery = (
  paginationOptions: ListOptions,
  filters: FamilyLocationFilter, // Changed from Ref<FamilyLocationFilter>
) => {
  const { familyLocation: familyLocationService } = useServices();
  return useQuery({
    queryKey: ['familyLocations', 'list', paginationOptions, filters],
    queryFn: async () => {
      const { page, itemsPerPage, sortBy } = unref(paginationOptions);
      const { familyId } = unref(filters);

      const listOptions: ListOptions = {
        page: page,
        itemsPerPage: itemsPerPage,
        sortBy: sortBy, // Pass the array directly
      };

      const filterOptions: FilterOptions = {
        familyId: familyId,
        locationType: filters.locationType,
        locationSource: filters.locationSource,
      };

      const response = await familyLocationService.search(listOptions, filterOptions);
      if (response.ok) {
        return response.value;
      } else {
        throw new Error(response.error?.message || 'Failed to load family locations');
      }
    },
    staleTime: 1000 * 60 * 1, // 1 minute
  });
};
