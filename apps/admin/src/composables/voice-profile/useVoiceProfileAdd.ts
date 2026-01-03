import { ref, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import type { IVoiceProfileFormInstance } from '@/components/voice-profile/VoiceProfileForm.vue';
import { usePreprocessAndCreateVoiceProfileMutation } from './usePreprocessAndCreateVoiceProfileMutation'; // Changed mutation
import type { PreprocessAndCreateVoiceProfileDto } from '@/types'; // Changed command type

interface UseVoiceProfileAddOptions {
  onSaveSuccess: () => void;
  onCancel: () => void;
  formRef: Ref<IVoiceProfileFormInstance | null>;
}

export function useVoiceProfileAdd(options: UseVoiceProfileAddOptions) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { mutate: preprocessAndCreateVoiceProfile, isPending: isAddingVoiceProfile } = usePreprocessAndCreateVoiceProfileMutation();

  const handleAddItem = async () => {
    if (!options.formRef.value) return;
    const { valid, errors } = await options.formRef.value.validate();
    if (!valid) {
      console.error('Validation errors:', errors);
      showSnackbar(t('common.formValidationErrors'), 'error');
      return;
    }

    const formData = options.formRef.value.getData();

    // Ensure memberId is present from the form data
    if (!formData.memberId) {
      showSnackbar(t('voiceProfile.errors.memberIdRequired'), 'error');
      return;
    }

    const command: PreprocessAndCreateVoiceProfileDto = {
      memberId: formData.memberId,
      label: formData.label,
      rawAudioUrls: formData.rawAudioUrls, // Use rawAudioUrls directly from formData
      language: formData.language,
      consent: formData.consent || false,
    };

    preprocessAndCreateVoiceProfile(command, {
      onSuccess: () => {
        showSnackbar(t('voiceProfile.messages.addSuccess'), 'success');
        options.onSaveSuccess();
      },
      onError: (error) => {
        console.error('Error creating voice profile:', error);
        showSnackbar(error.message || t('voiceProfile.messages.addError'), 'error');
      },
    });
  };

  const closeForm = () => {
    options.onCancel();
  };

  return {
    state: {
      isAddingVoiceProfile,
      isUploadingMedia: ref(false), // Placeholder for potential media upload
    },
    actions: {
      handleAddItem,
      closeForm,
    },
  };
}
