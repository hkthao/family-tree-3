import { reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMutation } from '@tanstack/vue-query';
import { ApiAiService } from '@/services/ai/api.ai.service';
import apiClient from '@/plugins/axios'; // Import apiClient to pass to service
import type { ApiError } from '@/types';

export function useGenerateFamilyKb(familyId: string) {
  const { t } = useI18n();

  const aiService = new ApiAiService(apiClient);

  const snackbar = reactive({
    show: false,
    message: '',
    color: 'success',
    timeout: 3000,
  });

  const generateKbMutation = useMutation({
    mutationFn: async (id: string) => {
      const result = await aiService.generateFamilyKb(id);
      if (!result.ok) {
        throw result.error;
      }
      return result.value;
    },
    onSuccess: () => {
      snackbar.message = t('family.generateKb.success');
      snackbar.color = 'success';
      snackbar.show = true;
    },
    onError: (error: ApiError) => {
      snackbar.message = error.message || t('family.generateKb.error');
      snackbar.color = 'error';
      snackbar.show = true;
    },
  });

  const triggerGenerateKb = () => {
    if (familyId) {
      generateKbMutation.mutate(familyId);
    } else {
      snackbar.message = t('family.generateKb.noFamilyId');
      snackbar.color = 'warning';
      snackbar.show = true;
    }
  };

  return {
    generateKbMutation,
    triggerGenerateKb,
    snackbar,
  };
}