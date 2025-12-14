import { computed, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useVuelidate } from '@vuelidate/core';
import { useProfileSettingsRules } from '@/validations/profile-settings.validation';
import { useGlobalSnackbar } from '@/composables';
import type { UpdateUserProfileDto, UserProfile } from '@/types';
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';

const PROFILE_QUERY_KEY = ['userProfile'];

export function useProfileSettings() {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient();

  // Initialize userService
  const userService = new ApiUserService(apiClient);

  // Form Data
  const formData = reactive({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    avatar: null as string | null, // Stores the URL of the current avatar from API
    avatarBase64: null as string | null, // Stores the new avatar as a base64 string
    externalId: '',
  });

  // Fetch User Profile
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

  watch(userProfile, (data) => {
    if (data) {
      Object.assign(formData, {
        firstName: data.firstName || '',
        lastName: data.lastName || '',
        email: data.email,
        phone: data.phone || '',
        avatar: data.avatar || null,
        externalId: data.externalId,
      });
    } else {
      // Optionally reset formData if userProfile becomes null (e.g., on logout or fetch error)
      Object.assign(formData, {
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        avatar: null,
        externalId: '',
      });
    }
  }, { immediate: true });

  watch(isFetchError, (isError) => {
    if (isError && fetchError.value) {
      showSnackbar(fetchError.value.message, 'error');
    }
  });

  // Validation
  const rules = useProfileSettingsRules();
  const v$ = useVuelidate(rules, formData);

  // Computed properties
  const generatedFullName = computed(() => {
    return `${formData.firstName} ${formData.lastName}`.trim();
  });

  const initialAvatarDisplay = computed(() => {
    return formData.avatarBase64 || formData.avatar;
  });

  // Update User Profile Mutation
  const { mutateAsync: updateProfile, isPending: isSavingProfile, isSuccess: isUpdateSuccess, isError: isUpdateError, error: updateError } = useMutation<UserProfile, Error, UpdateUserProfileDto>({
    mutationFn: async (updatedProfileDto: UpdateUserProfileDto) => {
      const result = await userService.updateUserProfile(updatedProfileDto);
      if (result.ok) {
        return result.value;
      } else {
        throw new Error(result.error?.message || t('userSettings.profile.saveError'));
      }
    },
  });

  watch(isUpdateSuccess, (success) => {
    if (success && userProfile.value) { // Ensure userProfile.value is available for updating cache
      queryClient.setQueryData(PROFILE_QUERY_KEY, userProfile.value); // Update cache with the latest data
      showSnackbar(t('userSettings.profile.saveSuccess'), 'success');
      // Update local avatar immediately if saved
      formData.avatar = userProfile.value.avatar || null;
      formData.avatarBase64 = null; // Clear base64 after successful upload
    }
  });

  watch(isUpdateError, (isError) => {
    if (isError && updateError.value) {
      showSnackbar(updateError.value.message, 'error');
    }
  });

  // Save Profile function
  const saveProfile = async () => {
    const isValid = await v$.value.$validate();
    if (!isValid) {
      showSnackbar(t('userSettings.profile.validationError'), 'error');
      return;
    }

    if (!userProfile.value) {
      showSnackbar(t('userSettings.profile.fetchError'), 'error'); // Should ideally not happen if data is loaded
      return;
    }

    const updatedProfile: UpdateUserProfileDto = {
      id: userProfile.value.id,
      externalId: userProfile.value.externalId,
      email: formData.email,
      name: generatedFullName.value,
      firstName: formData.firstName,
      lastName: formData.lastName,
      phone: formData.phone,
      avatarBase64: formData.avatarBase64,
    };

    await updateProfile(updatedProfile);
  };

  return {
    formData,
    v$,
    initialAvatarDisplay,
    isFetchingProfile,
    isSavingProfile,
    saveProfile,
    userProfile,
    isFetchError,
    fetchError,
  };
}