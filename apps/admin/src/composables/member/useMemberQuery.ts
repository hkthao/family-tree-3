import { useQuery } from '@tanstack/vue-query';
import { computed, unref } from 'vue';
import type { MaybeRef } from '@vueuse/core';
import type { Member, Result } from '@/types';
import { apiClient } from '@/plugins/axios';
import { ApiMemberService } from '@/services/member/api.member.service';
import type { IMemberService } from '@/services/member/member.service.interface';

const apiMemberService: IMemberService = new ApiMemberService(apiClient);

export const useMemberQuery = (memberId: MaybeRef<string | undefined>) => {
  const queryKey = computed(() => ['members', 'detail', unref(memberId)]);

  return useQuery({
    queryKey,
    queryFn: async () => {
      const id = unref(memberId);
      if (!id) return undefined; // Return undefined to match Member | undefined
      const result: Result<Member | undefined> = await apiMemberService.getById(id);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || `Failed to fetch member with ID ${id}`);
    },
    // The query will not execute until the memberId is defined
    enabled: computed(() => !!unref(memberId)),
  });
};
