import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';

export const useDeleteImageRestorationJobMutation = () => {
  const queryClient = useQueryClient();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const services = useServices();

  return useMutation<void, Error, { jobId: string }>({
    mutationFn: async ({ jobId }) => {
      const result = await services.imageRestorationJob.delete(jobId);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    onSuccess: () => {
      showSnackbar(t('imageRestorationJob.messages.deleteSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['image-restoration-jobs'] });
    },
    onError: (error) => {
      showSnackbar(error.message || t('imageRestorationJob.messages.deleteError'), 'error');
    },
  });
};