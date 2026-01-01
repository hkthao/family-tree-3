import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';
import { type ImageRestorationJobDto, type UpdateImageRestorationJobDto } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useUpdateImageRestorationJobMutation = () => {
  const queryClient = useQueryClient();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const services = useServices();

  return useMutation<ImageRestorationJobDto, Error, UpdateImageRestorationJobDto>({
    mutationFn: async (command) => {
      const result = await services.imageRestorationJob.update(command);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    onSuccess: (data) => {
      showSnackbar(t('imageRestorationJob.messages.updateSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['image-restoration-jobs'] });
      // Invalidate single item query too, using familyId from returned data
      if (data?.familyId && data?.jobId) {
        queryClient.invalidateQueries({ queryKey: ['image-restoration-job', data.familyId, data.jobId] });
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('imageRestorationJob.messages.updateError'), 'error');
    },
  });
};