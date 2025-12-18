import { computed, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useVuelidate } from '@vuelidate/core';
import { useProfileSettingsRules } from '@/validations/profile-settings.validation';
import { useGlobalSnackbar } from '@/composables';
import { useUserProfile } from '@/composables';
import type { UpdateUserProfileDto, UserProfile } from '@/types';
import { useMutation } from '@tanstack/vue-query';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';

export function useProfileSettings() {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  // Use the new useUserProfile composable
  const { userProfile, isFetchingProfile, isFetchError, fetchError, queryClient } = useUserProfile();

  // Initialize userService for update operations (fetch is now handled by useUserProfile)
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

  // Generated Full Name
  const generatedFullName = computed(() => {
    const parts = [];
    if (formData.firstName) parts.push(formData.firstName);
    if (formData.lastName) parts.push(formData.lastName);
    return parts.join(' ');
  });

  // Initial Avatar Display
  const initialAvatarDisplay = computed(() => formData.avatar);

  // Validation
  const rules = useProfileSettingsRules();
  const v$ = useVuelidate(rules, formData);

  // ... rest of the code ...
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
      queryClient.setQueryData(['userProfile'], userProfile.value); // Update cache with the latest data
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