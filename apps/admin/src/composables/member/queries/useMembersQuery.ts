import { useQuery } from '@tanstack/vue-query';
import { computed, unref } from 'vue';
import type { MaybeRef } from '@vueuse/core';
import type { MemberDto, MemberFilter, ListOptions, Result } from '@/types';
import type { IMemberService } from '@/services/member/member.service.interface';
import { useServices } from '@/plugins/services.plugin';



export const useMembersQuery = (
  paginationOptions: MaybeRef<ListOptions>,
  filters: MaybeRef<MemberFilter>,
  service: IMemberService = useServices().member,
) => {
  const queryKey = computed(() => [
    'members',
    'list',
    unref(paginationOptions).page,
    unref(paginationOptions).itemsPerPage,
    unref(paginationOptions).sortBy,
    unref(filters).searchQuery,
    unref(filters).gender,
    unref(filters).familyId,
    unref(filters).fatherId,
    unref(filters).motherId,
    unref(filters).husbandId,
    unref(filters).wifeId,
  ]);

  return useQuery<{ items: MemberDto[]; totalItems: number; totalPages: number }, Error>({
    queryKey,
    queryFn: async () => {
      const result: Result<{ items: MemberDto[]; totalItems: number; totalPages: number }> =
        await service.search(unref(paginationOptions), unref(filters));
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || 'Failed to fetch members');
    },
    select: (data) => data,
  });
};
