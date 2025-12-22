import { ref, watch } from 'vue';
import { useValidationRules } from '@/composables/utils/useValidationRules';
import { MediaType } from '@/types/enums';
import { useAddFamilyMediaFromUrlMutation } from '@/composables/family-media/mutations/useAddFamilyMediaFromUrlMutation';
import { getMediaTypeOptions } from '@/composables/utils/mediaTypeOptions';
import { type ComposerTranslation } from 'vue-i18n';
import { type VForm } from 'vuetify/components';

interface UseFamilyMediaAddLinkLogicDeps {
  familyId: string;
  t: ComposerTranslation;
  showSnackbar: (message: string, color: string) => void;
  rules: ReturnType<typeof useValidationRules>['rules'];
  emit: (event: 'close' | 'saved') => void;
}

export function useFamilyMediaAddLinkLogic(deps: UseFamilyMediaAddLinkLogicDeps) {
  const urlForm = ref<InstanceType<typeof VForm> | null>(null);
  const imagePreviewUrl = ref<string | null>(null);

  const urlFormData = ref({
    url: '',
    fileName: '',
    mediaType: MediaType.Image as MediaType, // Default to Image
    description: '',
  });

  const mediaTypeOptions = getMediaTypeOptions(deps.t).filter(option => option.value === MediaType.Image);

  const { mutate: addFamilyMediaFromUrl, isPending: isAddingUrl } = useAddFamilyMediaFromUrlMutation();

  watch(() => urlFormData.value.url, (newUrl) => {
    console.log('Watching URL:', newUrl);
    if (newUrl && (/\.(jpeg|jpg|png|gif|webp)(\?.*)?$/i.test(newUrl) || /fm=(jpeg|jpg|png|gif|webp)/i.test(newUrl))) {
      imagePreviewUrl.value = newUrl;
      console.log('Image preview URL set to:', imagePreviewUrl.value);
    } else {
      imagePreviewUrl.value = null;
      console.log('Image preview URL set to null. URL did not match regex or was empty.');
    }
  });


  const submitUrl = async () => {
    if (!urlForm.value) return;
    const { valid } = await urlForm.value.validate();
    if (valid) {
      const payload = {
        familyId: deps.familyId,
        url: urlFormData.value.url,
        fileName: urlFormData.value.fileName,
        mediaType: urlFormData.value.mediaType,
        description: urlFormData.value.description,
      };
      addFamilyMediaFromUrl(payload, {
        onSuccess: () => {
          deps.showSnackbar(deps.t('familyMedia.addLink.messages.success'), 'success');
          deps.emit('saved');
          // Clear form after successful upload
          urlFormData.value.url = '';
          urlFormData.value.fileName = '';
          urlFormData.value.mediaType = MediaType.Image; // Reset to Image
          urlFormData.value.description = '';
          urlForm.value?.resetValidation();
        },
        onError: (error) => {
          deps.showSnackbar(error.message || deps.t('familyMedia.addLink.messages.error'), 'error');
        },
      });
    }
  };

  return {
    state: {
      urlForm,
      urlFormData,
      mediaTypeOptions,
      isAddingUrl,
      imagePreviewUrl,
    },
    actions: {
      submitUrl,
    },
  };
}