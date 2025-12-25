import { ref, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useMutation } from '@tanstack/vue-query';

interface UseChatInputProps {
  modelValue?: string;
  disabled: boolean;
}

interface UseChatInputEmits {
  (event: 'update:modelValue', value: string): void;
  (event: 'sendMessage'): void;
  (event: 'addAttachment', type: 'image' | 'pdf'): void;
  (event: 'addLocation', location: { latitude: number; longitude: number; address?: string }): void;
}

export function useChatInput(props: UseChatInputProps, emit: UseChatInputEmits) {
  const { modelValue, disabled } = toRefs(props);
  const { t } = useI18n();
  const { chat: chatService } = useServices();
  const { showSnackbar } = useGlobalSnackbar();

  const selectedLocation = ref<{ latitude: number; longitude: number; address?: string } | null>(null);

  const updateModelValue = (value: string) => {
    emit('update:modelValue', value);
  };

  const handleEnterKey = (event: KeyboardEvent) => {
    if (!event.shiftKey) {
      event.preventDefault();
      sendMessage();
    }
  };

  const sendMessage = () => {
    const currentModelValue = modelValue?.value ?? '';
    if ((currentModelValue.trim() || selectedLocation.value) && !disabled.value) {
      if (selectedLocation.value) {
        emit('addLocation', selectedLocation.value);
        clearSelectedLocation();
      }
      emit('sendMessage');
    }
  };

  const ocrMutation = useMutation({
    mutationFn: (file: File) => chatService.performOcr(file),
    onSuccess: (result) => {
      if (result.ok) {
        const ocrText = result.value.text;
        const currentModelValue = modelValue?.value ?? '';
        emit('update:modelValue', `${currentModelValue + '\n'}${ocrText}`);
        showSnackbar(t('chatInput.ocrSuccess'), 'success');
      } else {
        const apiError = result.error as ApiError;
        showSnackbar(apiError.message || t('chatInput.ocrFailed'), 'error');
      }
    },
    onError: (error) => {
      console.error('OCR API call failed:', error);
      showSnackbar(t('chatInput.ocrFailed'), 'error');
    },
  });

  const addImagePdf = (file: File) => {
    ocrMutation.mutate(file);
  };

  const getCurrentLocation = () => {
    console.log('Get Current Location clicked');
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const { latitude, longitude } = position.coords;
          console.log(`Latitude: ${latitude}, Longitude: ${longitude}`);
          selectedLocation.value = { latitude, longitude };
          showSnackbar(t('chatInput.locationSelected'), 'success');
        },
        (error) => {
          console.error('Error getting location:', error);
          showSnackbar(t('chatInput.errors.locationAccessDenied'), 'error');
        }
      );
    } else {
      console.error('Geolocation is not supported by this browser.');
      showSnackbar(t('chatInput.errors.geolocationNotSupported'), 'error');
    }
  };

  const clearSelectedLocation = () => {
    selectedLocation.value = null;
  };

  return {
    updateModelValue,
    handleEnterKey,
    sendMessage,
    addImagePdf,
    getCurrentLocation,
    clearSelectedLocation,
    isPerformingOcr: ocrMutation.isPending,
    selectedLocation,
  };
}
