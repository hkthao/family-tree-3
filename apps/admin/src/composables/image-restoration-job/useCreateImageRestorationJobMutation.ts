import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';
import { type ImageRestorationJobDto } from '@/types'; // Removed CreateImageRestorationJobDto
import { useServices } from '@/plugins/services.plugin';

interface CreateImageRestorationJobMutationParams {
  file: File;
  familyId: string;
  useCodeformer: boolean;
}

export const useCreateImageRestorationJobMutation = () => {
  const queryClient = useQueryClient();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const services = useServices();

  return useMutation<ImageRestorationJobDto, Error, CreateImageRestorationJobMutationParams>({
    mutationFn: async (params) => {
      const result = await services.imageRestorationJob.add(params.file, params.familyId, params.useCodeformer);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    onSuccess: () => {
      showSnackbar(t('imageRestorationJob.messages.createSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['image-restoration-jobs'] });
    },
    onError: (error) => {
      showSnackbar(error.message || t('imageRestorationJob.messages.createError'), 'error');
    },
  });
};