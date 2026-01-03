import { ref, watch, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import type { IVoiceProfileFormInstance } from '@/components/voice-profile/VoiceProfileForm.vue';
import { useUpdateVoiceProfileMutation } from './useUpdateVoiceProfileMutation';
import { useVoiceProfileDetail } from './useVoiceProfileDetail';
import type { UpdateVoiceProfileCommand } from '@/types';

interface UseVoiceProfileEditOptions {
  voiceProfileId: string;
  onSaveSuccess: () => void;
  onCancel: () => void;
  formRef: Ref<IVoiceProfileFormInstance | null>;
}

export function useVoiceProfileEdit(options: UseVoiceProfileEditOptions) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { mutate: updateVoiceProfile, isPending: isUpdatingVoiceProfile } = useUpdateVoiceProfileMutation();

  const voiceProfileIdRef = ref(options.voiceProfileId);

  const { state: { voiceProfile, isLoading: isLoadingVoiceProfile }, actions: { refetch } } = useVoiceProfileDetail({
    voiceProfileId: voiceProfileIdRef,
    onClose: options.onCancel,
  });

  watch(() => options.voiceProfileId, (newId) => {
    voiceProfileIdRef.value = newId;
    refetch();
  });

  const handleUpdateItem = async () => {
    if (!options.formRef.value) return;

    const { valid, errors } = await options.formRef.value.validate();
    if (!valid) {
      console.error('Validation errors:', errors);
      showSnackbar(t('common.formValidationErrors'), 'error');
      return;
    }

    const formData = options.formRef.value.getData();
    const command: UpdateVoiceProfileCommand = {
      id: options.voiceProfileId,
      label: formData.label,
      audioUrl: formData.rawAudioUrls[0] || '',
      durationSeconds: formData.durationSeconds,
      language: formData.language,
      consent: formData.consent,
      status: formData.status,
    };

    updateVoiceProfile({ id: options.voiceProfileId, data: command }, {
      onSuccess: () => {
        showSnackbar(t('voiceProfile.messages.editSuccess'), 'success');
        options.onSaveSuccess();
      },
      onError: (error) => {
        console.error('Error updating voice profile:', error);
        showSnackbar(error.message || t('voiceProfile.messages.editError'), 'error');
      },
    });
  };

  const closeForm = () => {
    options.onCancel();
  };

  return {
    state: {
      voiceProfile,
      isLoading: isLoadingVoiceProfile,
      isUpdatingVoiceProfile,
      isUploadingMedia: ref(false), // Placeholder for potential media upload
    },
    actions: {
      handleUpdateItem,
      closeForm,
    },
  };
}
