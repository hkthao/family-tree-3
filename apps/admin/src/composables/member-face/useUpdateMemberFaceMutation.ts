import { useMutation } from '@tanstack/vue-query';
import type { MemberFace } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useUpdateMemberFaceMutation = () => {
  const services = useServices();
  return useMutation({
    mutationFn: async (memberFace: MemberFace) => {
      const response = await services.memberFace.update(memberFace);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to update member face');
    },
  });
};
