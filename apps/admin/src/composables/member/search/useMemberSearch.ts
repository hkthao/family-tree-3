import { ref, watch, computed, unref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { ApiMemberService } from '@/services/member/api.member.service';
import apiClient from '@/plugins/axios';
import type { MemberDto } from '@/types';
import type { MaybeRef } from '@vueuse/core'; // Assuming @vueuse/core is available for MaybeRef or define custom

// Instantiate the service outside the composable to avoid re-instantiation on every call
const memberService = new ApiMemberService(apiClient);

interface UseMemberSearchOptions {
  debounceTime?: number;
  familyId?: MaybeRef<string | undefined | null>;
}

export function useMemberSearch(options?: UseMemberSearchOptions) {
  const { debounceTime = 300, familyId: initialFamilyId } = options || {};

  const searchTerm = ref('');
  const debouncedSearchTerm = ref('');
  const currentFamilyId = ref(unref(initialFamilyId));

  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  watch(
    () => options?.familyId,
    (newFamilyId) => {
      currentFamilyId.value = unref(newFamilyId);
    },
    { immediate: true }
  );

  const { data, isLoading, isFetching, error } = useQuery<MemberDto[], Error>({
    queryKey: ['members', debouncedSearchTerm, currentFamilyId],
    queryFn: async () => {
      const filters: { [key: string]: any } = {};
      if (debouncedSearchTerm.value) {
        filters.fullName = debouncedSearchTerm.value;
      }
      if (currentFamilyId.value) {
        filters.familyId = currentFamilyId.value;
      }

      // Only perform search if there's a search term or a familyId, otherwise return empty
      if (!debouncedSearchTerm.value && !currentFamilyId.value) {
        return [];
      }

      const result = await memberService.search(
        { page: 1, itemsPerPage: 10 },
        filters
      );
      if (result.ok) {
        return result.value.items;
      }
      throw result.error;
    },
    staleTime: 1000 * 60 * 1, // 1 minute
    // Only enabled if there's a debounced search term OR a family ID
    enabled: computed(() => !!debouncedSearchTerm.value || !!currentFamilyId.value),
  });

  watch(searchTerm, (newSearchTerm) => {
    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }
    debounceTimer = setTimeout(() => {
      debouncedSearchTerm.value = newSearchTerm;
    }, debounceTime);
  });

  return {
    searchTerm,
    members: data,
    isLoading: computed(() => isLoading.value || isFetching.value),
    error,
  };
}
