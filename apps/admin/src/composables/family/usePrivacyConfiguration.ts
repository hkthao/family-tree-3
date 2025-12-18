import { ref, watch, computed, unref, type MaybeRef } from 'vue';
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/composables';
import type { PrivacyConfiguration } from '@/types/privacyConfiguration.d';
import { queryKeys } from '@/constants/queryKeys';

export function usePrivacyConfiguration(familyId: MaybeRef<string>) {
  const { t } = useI18n();
  const { family } = useServices(); // Assuming 'family' service handles privacy config
  const queryClient = useQueryClient();

  const initialPublicProperties = ref<string[]>([]);

  // Fetch privacy configuration
  const { data: privacyConfiguration, isLoading, isError, error } = useQuery<PrivacyConfiguration, Error>({
    queryKey: [queryKeys.privacyConfiguration.detail(unref(familyId))],
    queryFn: async () => {
      const currentFamilyId = unref(familyId);
      if (!currentFamilyId) return Promise.reject(new Error(t('family.privacy.familyIdRequired')));
      const result = await family.getPrivacyConfiguration(currentFamilyId); // Assuming service method
      if (result.ok) {
        initialPublicProperties.value = result.value.publicMemberProperties;
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!unref(familyId)), // Only run query if familyId is available
    staleTime: Infinity, // Privacy settings don't change often
  });

  // Mutation for updating privacy configuration
  const { mutateAsync: updatePrivacyMutation, isPending: isUpdating } = useMutation<void, Error, { familyId: string; publicMemberProperties: string[] }>({
    mutationFn: async ({ familyId: mutationFamilyId, publicMemberProperties }) => {
      const result = await family.updatePrivacyConfiguration(mutationFamilyId, publicMemberProperties);
      if (result.ok) {
        return; // Return void
      } else {
        throw result.error;
      }
    },
    onSuccess: (_, variables) => {
      // Manually update the cache with the new properties
      queryClient.setQueryData([queryKeys.privacyConfiguration.detail(variables.familyId)], (old: PrivacyConfiguration | undefined) => {
        if (old) {
          return { ...old, publicMemberProperties: variables.publicMemberProperties };
        }
        return undefined; // Or some default if old data is not found
      });
      initialPublicProperties.value = variables.publicMemberProperties;
    },
    onError: (err) => {
      // Revert to initial state on error
      queryClient.setQueryData([queryKeys.privacyConfiguration.detail(unref(familyId))], (oldData: PrivacyConfiguration | undefined) => {
        if (oldData) {
          return { ...oldData, publicMemberProperties: initialPublicProperties.value };
        }
        return oldData;
      });
    },
  });

  const updatePrivacySettings = async (publicMemberProperties: string[]) => {
    const currentFamilyId = unref(familyId);
    if (!currentFamilyId) {
      throw new Error(t('family.privacy.familyIdRequired'));
    }
    await updatePrivacyMutation({ familyId: currentFamilyId, publicMemberProperties });
  };

  return {
    privacyConfiguration,
    isLoading,
    isUpdating,
    error,
    updatePrivacySettings,
    initialPublicProperties, // Expose initial properties for potential revert logic
  };
}
