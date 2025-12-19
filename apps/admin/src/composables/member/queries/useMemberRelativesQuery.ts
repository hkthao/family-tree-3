import { useQuery } from '@tanstack/vue-query';
import { computed, unref } from 'vue';
import type { MaybeRef } from '@vueuse/core';
import type { Member, Result } from '@/types';
import { apiClient } from '@/plugins/axios';
import { ApiMemberService } from '@/services/member/api.member.service';
import type { IMemberService } from '@/services/member/member.service.interface';

const apiMemberService: IMemberService = new ApiMemberService(apiClient);

export const useMemberRelativesQuery = (memberId: MaybeRef<string | undefined>) => {
  const queryKey = computed(() => ['members', 'relatives', unref(memberId)]);

  return useQuery({
    queryKey,
    queryFn: async () => {
      const id = unref(memberId);
      if (!id) return null;
      const result: Result<Member[]> = await apiMemberService.getRelatives(id);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || `Failed to fetch relatives for member ID ${id}`);
    },
    enabled: computed(() => !!unref(memberId)),
  });
};
