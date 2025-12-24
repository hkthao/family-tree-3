import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { MemberDto, MemberUpdateDto } from '@/types';
import { apiClient } from '@/plugins/axios';
import { ApiMemberService } from '@/services/member/api.member.service';
import type { IMemberService } from '@/services/member/member.service.interface';

const apiMemberService: IMemberService = new ApiMemberService(apiClient);

export const useUpdateMemberMutation = () => {
  const queryClient = useQueryClient();

  return useMutation<MemberDto, Error, MemberUpdateDto>({
    mutationFn: async (updatedMemberData: MemberUpdateDto) => {
      const result = await apiMemberService.update(updatedMemberData);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || 'Failed to update member');
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['members', 'list'] });
      queryClient.invalidateQueries({ queryKey: ['members', 'detail', variables.id] });
      // Optionally invalidate other related queries if needed
    },
  });
};
