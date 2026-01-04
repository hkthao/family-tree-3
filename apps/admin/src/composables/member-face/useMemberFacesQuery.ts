import { useQuery } from '@tanstack/vue-query';
import { apiClient } from '@/plugins/axios';
import { ApiMemberFaceService } from '@/services/member-face/api.member-face.service';

const memberFaceService = new ApiMemberFaceService(apiClient);

export function useMemberFacesQuery(memberId: string) {
  return useQuery({
    queryKey: ['member-faces', memberId],
    queryFn: async () => {
      const response = await memberFaceService.getMemberFacesByMemberId(memberId);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to fetch member faces');
    },
    enabled: !!memberId,
  });
}