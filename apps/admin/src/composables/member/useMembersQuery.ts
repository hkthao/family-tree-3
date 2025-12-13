import { useQuery } from '@tanstack/vue-query';
import { computed, unref } from 'vue';
import type { MaybeRef } from '@vueuse/core';
import type { Member, MemberFilter, ListOptions, Result } from '@/types';
import { apiClient } from '@/plugins/axios';
import { ApiMemberService } from '@/services/member/api.member.service';
import type { IMemberService } from '@/services/member/member.service.interface';

const apiMemberService: IMemberService = new ApiMemberService(apiClient);

export const useMembersQuery = (
  paginationOptions: MaybeRef<ListOptions>,
  filters: MaybeRef<MemberFilter>,
) => {
  const queryKey = computed(() => [
    'members',
    'list',
    unref(paginationOptions),
    unref(filters),
  ]);

  return useQuery({
    queryKey,
    queryFn: async () => {
      const result: Result<{ items: Member[]; totalItems: number; totalPages: number }> =
        await apiMemberService.search(unref(paginationOptions), unref(filters));
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || 'Failed to fetch members');
    },
    select: (data) => data,
  });
};
