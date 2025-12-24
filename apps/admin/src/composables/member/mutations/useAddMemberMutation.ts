import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { MemberDto, MemberAddDto } from '@/types';
import { apiClient } from '@/plugins/axios';
import { ApiMemberService } from '@/services/member/api.member.service';
import type { IMemberService } from '@/services/member/member.service.interface';

const apiMemberService: IMemberService = new ApiMemberService(apiClient);

export const useAddMemberMutation = () => {
  const queryClient = useQueryClient();

  return useMutation<MemberDto, Error, MemberAddDto>({
    mutationFn: async (newMemberData: MemberAddDto) => {
      const result = await apiMemberService.add(newMemberData);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || 'Failed to add member');
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['members', 'list'] });
      // Optionally invalidate other related queries if needed
    },
  });
};
