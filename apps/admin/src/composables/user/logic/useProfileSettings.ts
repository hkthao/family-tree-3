import { computed, reactive, watch, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useProfileSettingsRules } from '@/validations/profile-settings.validation';
import { useGlobalSnackbar } from '@/composables';
import { useUserProfile } from '@/composables';
import type { UpdateUserProfileDto, UserProfile } from '@/types';
import { useMutation } from '@tanstack/vue-query';
import type { IUserService } from '@/services/user/user.service.interface';
import { useServices } from '@/composables';
import type { VForm } from 'vuetify/components';

interface UseProfileSettingsOptions {
  userService?: IUserService;
}

export function useProfileSettings(options: UseProfileSettingsOptions = {}) {
  const { userService = useServices().user } = options;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const formRef = ref<VForm | null>(null);

  // Use the new useUserProfile composable
  const { state: { userProfile, isFetchingProfile, isFetchError, fetchError, queryClient } } = useUserProfile();

  // Initialize userService for update operations (fetch is now handled by useUserProfile)


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

  const handleUserProfileChange = (data?: UserProfile) => {
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
  };

  watch(userProfile, handleUserProfileChange, { immediate: true });

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
  const validationRules = useProfileSettingsRules();

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

  const handleUpdateSuccess = (success: boolean) => {
    if (success && userProfile.value) { // Ensure userProfile.value is available for updating cache
      queryClient.setQueryData(['userProfile'], userProfile.value); // Update cache with the latest data
      showSnackbar(t('userSettings.profile.saveSuccess'), 'success');
      // Update local avatar immediately if saved
      formData.avatar = userProfile.value.avatar || null;
      formData.avatarBase64 = null; // Clear base64 after successful upload
    }
  };

  watch(isUpdateSuccess, handleUpdateSuccess);

  const handleUpdateError = (isError: boolean) => {
    if (isError && updateError.value) {
      showSnackbar(updateError.value.message, 'error');
    }
  };

  watch(isUpdateError, handleUpdateError);

  // Save Profile function
  const saveProfile = async () => {
    if (!formRef.value) {
      showSnackbar(t('userSettings.profile.validationError'), 'error');
      return;
    }
    const { valid } = await formRef.value.validate();
    if (!valid) {
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
    state: {
      formData,
      formRef,
      initialAvatarDisplay,
      isFetchingProfile,
      isSavingProfile,
      userProfile,
      isFetchError,
      fetchError,
      validationRules,
    },
    actions: {
      saveProfile,
    },
  };
}