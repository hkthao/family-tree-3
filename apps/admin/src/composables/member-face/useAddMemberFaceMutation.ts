import { useMutation } from '@tanstack/vue-query';
import type { MemberFace } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useAddMemberFaceMutation = () => {
  const services = useServices();
  return useMutation({
    mutationFn: async (memberFace: Omit<MemberFace, 'id'>) => {
      const response = await services.memberFace.add(memberFace);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to add member face');
    },
  });
};
