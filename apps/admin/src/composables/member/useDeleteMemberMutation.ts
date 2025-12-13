import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { apiClient } from '@/plugins/axios';
import { ApiMemberService } from '@/services/member/api.member.service';
import type { IMemberService } from '@/services/member/member.service.interface';

const apiMemberService: IMemberService = new ApiMemberService(apiClient);

export const useDeleteMemberMutation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (memberId: string) => {
      const result = await apiMemberService.delete(memberId);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || `Failed to delete member with ID ${memberId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['members', 'list'] });
      // Optionally invalidate other related queries if needed
    },
  });
};
