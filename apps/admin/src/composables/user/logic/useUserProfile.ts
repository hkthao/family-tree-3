import { watch } from 'vue';
import type { UserProfile } from '@/types';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useServices } from '@/plugins/services.plugin';
import type { IUserService } from '@/services/user/user.service.interface';

const PROFILE_QUERY_KEY = ['userProfile'];

interface UseUserProfileOptions {
  userService?: IUserService;
}

export function useUserProfile(options: UseUserProfileOptions = {}) {
  const { userService = useServices().user } = options;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient();

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

  const handleFetchErrorChange = (isError: boolean) => {
    if (isError && fetchError.value) {
      showSnackbar(fetchError.value.message, 'error');
    }
  };

  watch(isFetchError, handleFetchErrorChange);

  return {
    state: {
      userProfile,
      isFetchingProfile,
      isFetchError,
      fetchError,
      queryClient,
    },
  };
}
