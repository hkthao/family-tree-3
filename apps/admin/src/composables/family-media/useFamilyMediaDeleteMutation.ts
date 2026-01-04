// apps/admin/src/composables/family-media/useFamilyMediaDeleteMutation.ts
import { useMutation } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { Result } from '@/types';

export const useFamilyMediaDeleteMutation = () => {
  const services = useServices();

  return useMutation<Result<boolean>, Error, string>({ // Last arg is the media ID to delete
    mutationFn: async (id: string) => {
      const response = await services.familyMedia.delete(id);
      if (response.ok) {
        return response;
      }
      throw new Error(response.error?.message || 'Failed to delete media item');
    },
  });
};
