import { ref, computed, unref, type MaybeRef, reactive } from 'vue';
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';
import type { PrivacyConfiguration } from '@/types/privacyConfiguration.d';
import { queryKeys } from '@/constants/queryKeys';

export function usePrivacyConfiguration(familyId: MaybeRef<string>) {
  const { t } = useI18n();
  const { family } = useServices(); // Assuming 'family' service handles privacy config
  const queryClient = useQueryClient();

  // Store the initial state of all public properties for rollback on error
  const initialPublicProperties = reactive<PrivacyConfiguration>({
    id: '',
    familyId: unref(familyId) || '',
    publicMemberProperties: [],
    publicEventProperties: [],
    publicFamilyProperties: [],
    publicFamilyLocationProperties: [],
    publicMemoryItemProperties: [],
    publicMemberFaceProperties: [],
    publicFoundFaceProperties: [],
  });

  // Fetch privacy configuration
  const { data: privacyConfiguration, isLoading, error } = useQuery<PrivacyConfiguration, Error>({
    queryKey: [queryKeys.privacyConfiguration.detail(unref(familyId))],
    queryFn: async () => {
      const currentFamilyId = unref(familyId);
      if (!currentFamilyId) return Promise.reject(new Error(t('family.privacy.familyIdRequired')));
      const result = await family.getPrivacyConfiguration(currentFamilyId); // Assuming service method
      if (result.ok) {
        // Populate initialPublicProperties with fetched data
        Object.assign(initialPublicProperties, result.value);
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!unref(familyId)), // Only run query if familyId is available
    staleTime: Infinity, // Privacy settings don't change often
  });

  // Mutation for updating privacy configuration
  const { mutateAsync: updatePrivacyMutation, isPending: isUpdating } = useMutation<void, Error, PrivacyConfiguration>({
    mutationFn: async (settingsToSave) => {
      const result = await family.updatePrivacyConfiguration(settingsToSave.familyId, settingsToSave); // Pass the whole object
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
          return { ...old, ...variables }; // Merge all updated properties
        }
        return undefined;
      });
      // Update initial state to reflect successful save
      Object.assign(initialPublicProperties, variables);
    },
    onError: (_err) => {
      // Revert to initial state on error
      queryClient.setQueryData([queryKeys.privacyConfiguration.detail(unref(familyId))], (oldData: PrivacyConfiguration | undefined) => {
        if (oldData) {
          return { ...oldData, ...initialPublicProperties }; // Revert all properties
        }
        return oldData;
      });
    },
  });

  const updatePrivacySettings = async (settingsToSave: Omit<PrivacyConfiguration, 'id'>) => {
    const currentFamilyId = unref(familyId);
    if (!currentFamilyId) {
      throw new Error(t('family.privacy.familyIdRequired'));
    }
    await updatePrivacyMutation({ id: privacyConfiguration.value?.id || '', ...settingsToSave });
  };

  return {
    state: {
      privacyConfiguration,
      isLoading,
      isUpdating,
      error,
    },
    actions: {
      updatePrivacySettings,
    },
  };
}
