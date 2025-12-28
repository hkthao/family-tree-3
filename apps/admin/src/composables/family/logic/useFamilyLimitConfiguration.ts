import { watch, toValue } from 'vue';
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyLimitConfiguration, UpdateFamilyLimitConfigurationDto } from '@/types/family.d';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';

export function useFamilyLimitConfiguration(familyId: string) {
  const { t } = useI18n();
  const { family: familyService } = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient();

  const queryKey = ['familyLimitConfiguration', familyId];

  const { isLoading, error, data: familyLimitData } = useQuery<FamilyLimitConfiguration>({
    queryKey: queryKey,
    queryFn: async () => {
      const result = await familyService.getFamilyLimitConfiguration(toValue(familyId));
      if (result.ok) {
        return result.value;
      } else {
        throw new Error(result.error.message || t('family.form.errorLoadLimits'));
      }
    },
    enabled: !!toValue(familyId),
  });

  const updateMutation = useMutation({
    mutationFn: async (payload: UpdateFamilyLimitConfigurationDto) => {
      const result = await familyService.updateFamilyLimitConfiguration(toValue(familyId), payload);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error?.message || t('family.form.errorSaveLimits'));
    },
    onSuccess: (data) => {
      queryClient.setQueryData(queryKey, data);
      showSnackbar(t('family.form.saveLimitSuccess'), 'success');
    },
    onError: (err) => {
      showSnackbar(err.message, 'error');
    },
  });

  const updateFamilyLimits = (payload: UpdateFamilyLimitConfigurationDto) => {
    updateMutation.mutate(payload);
  };

  watch(() => toValue(familyId), (newFamilyId) => {
    if (newFamilyId) {
      queryClient.invalidateQueries({ queryKey: queryKey });
    }
  });

  return {
    isLoading,
    error,
    familyLimitData,
    isUpdating: updateMutation.isPending,
    updateFamilyLimits,
    updateMutation, // Export updateMutation for external access
  };
}
