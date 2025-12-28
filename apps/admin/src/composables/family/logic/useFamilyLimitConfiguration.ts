import { toValue, type Ref, computed } from 'vue'; // Added computed
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyLimitConfiguration, UpdateFamilyLimitConfigurationDto } from '@/types/family.d';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';

export function useFamilyLimitConfiguration(familyId: Ref<string>) {
  const { t } = useI18n();
  const { family: familyService } = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient();

  // Make queryKey reactive
  const reactiveQueryKey = computed(() => ['familyLimitConfiguration', toValue(familyId)]);

  const { isLoading, error, data: familyLimitData } = useQuery<FamilyLimitConfiguration>({
    queryKey: reactiveQueryKey, // Use reactive queryKey
    queryFn: async () => {
      // Ensure familyIdRef.value is not null before making API call
      const id = toValue(familyId);
      if (!id) {
        throw new Error("Family ID is missing for fetching limits.");
      }
      const result = await familyService.getFamilyLimitConfiguration(id);
      if (result.ok) {
        return result.value;
      } else {
        throw new Error(result.error.message || t('family.form.errorLoadLimits'));
      }
    },
    enabled: computed(() => !!toValue(familyId)), // Make enabled reactive
  });

  const updateMutation = useMutation({
    mutationFn: async (payload: UpdateFamilyLimitConfigurationDto) => {
      const result = await familyService.updateFamilyLimitConfiguration(toValue(familyId), payload);
      if (result.ok) {
        return result.value;
      }
      throw new Error(result.error.message || t('family.form.errorSaveLimits'));
    },
    onSuccess: (data, _variables, _context) => {
      queryClient.setQueryData(reactiveQueryKey.value, data); // Use reactiveQueryKey.value here
      showSnackbar(t('family.form.saveLimitSuccess'), 'success');
    },
    onError: (err, _variables, _context) => {
      showSnackbar(err.message, 'error');
    },
  });

  const updateFamilyLimits = (
    payload: UpdateFamilyLimitConfigurationDto,
    options?: { onSuccess?: (data: FamilyLimitConfiguration) => void; onError?: (error: Error) => void }
  ) => {
    updateMutation.mutate(payload, {
      onSuccess: options?.onSuccess ? (data: any) => options.onSuccess?.(data as FamilyLimitConfiguration) : undefined,
      onError: options?.onError ? (error: any) => options.onError?.(error as Error) : undefined,
    });
  };

  // Removed the manual watch for familyId changes and invalidateQueries
  // useQuery with a reactive queryKey automatically handles refetching when its dependencies change.

  return {
    isLoading,
    error,
    familyLimitData,
    isUpdating: updateMutation.isPending,
    updateFamilyLimits,
  };
}
