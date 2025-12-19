import { watch } from 'vue';
import type { UserProfile } from '@/types';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';

const PROFILE_QUERY_KEY = ['userProfile'];

export function useUserProfile() {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient();

  const userService = new ApiUserService(apiClient);

  const { isLoading: isFetchingProfile, data: userProfile, isError: isFetchError, error: fetchError } = useQuery<UserProfile, Error>({
    queryKey: PROFILE_QUERY_KEY,
    queryFn: async () => {
      const result = await userService.getCurrentUserProfile();
      if (result.ok) {
        return result.value;
      } else {
        throw new Error(result.error?.message || t('userSettings.profile.fetchError'));
      }
    },
  });

  watch(isFetchError, (isError) => {
    if (isError && fetchError.value) {
      showSnackbar(fetchError.value.message, 'error');
    }
  });

  return {
    userProfile,
    isFetchingProfile,
    isFetchError,
    fetchError,
    queryClient, // Expose queryClient if needed for invalidation elsewhere
  };
}
