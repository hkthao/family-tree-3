// apps/admin/src/composables/family-media/useFamilyMediaUploadMutation.ts
import { useMutation } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyMedia } from '@/types';
import type { Result } from '@/types';

interface UploadCommand {
  familyId: string;
  file: File;
  description?: string;
}

export const useFamilyMediaUploadMutation = () => {
  const services = useServices();

  return useMutation<Result<FamilyMedia>, Error, UploadCommand>({
    mutationFn: async (command: UploadCommand) => {
      const response = await services.familyMedia.create(command.familyId, command.file, command.description);
      if (response.ok) {
        return response;
      }
      throw new Error(response.error?.message || 'Failed to upload media file');
    },
  });
};
