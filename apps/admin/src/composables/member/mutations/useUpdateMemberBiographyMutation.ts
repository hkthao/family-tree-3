import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { apiClient } from '@/plugins/axios';
import { ApiMemberService } from '@/services/member/api.member.service';
import type { IMemberService } from '@/services/member/member.service.interface';

const apiMemberService: IMemberService = new ApiMemberService(apiClient);

export const useUpdateMemberBiographyMutation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ memberId, biographyContent }: { memberId: string; biographyContent: string }) => {
      const result = await apiMemberService.updateMemberBiography(memberId, biographyContent);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || `Failed to update biography for member ID ${memberId}`);
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['members', 'detail', variables.memberId] });
      // Invalidate the specific member's detail query
    },
  });
};
