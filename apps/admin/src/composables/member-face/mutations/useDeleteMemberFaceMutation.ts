import { useMutation } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';

export const useDeleteMemberFaceMutation = () => {
  const services = useServices();
  return useMutation({
    mutationFn: async (id: string) => {
      const response = await services.memberFace.delete(id);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to delete member face');
    },
  });
};