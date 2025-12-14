import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError, UserPreference, Result } from '@/types';
import { computed, watch } from 'vue';
import i18n from '@/plugins/i18n'; // Import global i18n instance
import { Language } from '@/types';

export function useUserPreferences() {
  const { user } = useServices();
  const queryClient = useQueryClient();

  // Query to fetch user preferences
  const { data: preferences, isLoading, isError, error, refetch } = useQuery<
    UserPreference,
    ApiError
  >({
    queryKey: queryKeys.userSettings.preferences,
    queryFn: async () => {
      const result = await user.getUserPreferences();
      if (result.ok) {
        // Set i18n locale based on fetched preferences
        i18n.global.locale.value =
          result.value.language === Language.English ? 'en' : 'vi';
        return result.value;
      }
      throw result.error;
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  // Mutation to save user preferences
  const {
    mutate: savePreferences,
    isPending: isSavingMutation, // Use isPending as isLoading is deprecated
    isError: isSaveError,
    error: saveError,
  } = useMutation<Result<boolean, ApiError>, ApiError, UserPreference>({
    mutationFn: async (newPreferences: UserPreference) => {
      const result = await user.saveUserPreferences(newPreferences);
      if (!result.ok) {
        throw result.error;
      }
      return { ok: true, value: true }; // Explicitly return Result<boolean, ApiError>
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.userSettings.preferences });
    },
  });

  // Computed ref for current language based on preferences
  const currentChatLanguage = computed(() => {
    return preferences.value?.language === Language.English ? 'en' : 'vi';
  });

  // Watch for changes in preferences and update i18n locale
  watch(
    () => preferences.value?.language,
    (newLang) => {
      if (newLang) {
        i18n.global.locale.value = (newLang as Language) === Language.English ? 'en' : 'vi';
      }
    },
  );

  return {
    preferences,
    isLoading,
    isError,
    error,
    refetch,
    savePreferences,
    isSaving: isSavingMutation, // Expose the renamed isLoading
    isSaveError,
    saveError,
    currentChatLanguage,
  };
}
