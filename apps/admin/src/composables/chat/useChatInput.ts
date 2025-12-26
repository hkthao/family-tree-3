import { ref, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useMutation } from '@tanstack/vue-query';
import { useMapLocationDrawerStore } from '@/stores/mapLocationDrawer.store';

interface LocationData {
  latitude: number;
  longitude: number;
  address?: string;
}

interface SelectedLocation extends LocationData {
  source: 'current' | 'map';
}

interface UseChatInputProps {
  modelValue?: string;
  disabled: boolean;
}

interface UseChatInputEmits {
  (event: 'update:modelValue', value: string): void;
  (event: 'sendMessage'): void;
  (event: 'addAttachment', type: 'image' | 'pdf'): void;
  (event: 'addLocation', location: LocationData): void;
}

export function useChatInput(props: UseChatInputProps, emit: UseChatInputEmits) {
  const { modelValue, disabled } = toRefs(props);
  const { t } = useI18n();
  const { chat: chatService } = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const mapDrawerStore = useMapLocationDrawerStore();

  const selectedLocation = ref<SelectedLocation | null>(null);

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
          selectedLocation.value = { latitude, longitude, source: 'current' };
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

  const openMapPicker = async (initialLocation?: LocationData) => {
    try {
      const result = await mapDrawerStore.openDrawer(initialLocation);
      if (result.coordinates) {
        selectedLocation.value = {
          latitude: result.coordinates.latitude,
          longitude: result.coordinates.longitude,
          address: result.location,
          source: 'map',
        };
        showSnackbar(t('chatInput.locationSelectedFromMap'), 'success');
      }
    } catch (error) {
      console.error('Map location selection cancelled or failed:', error);
      showSnackbar(t('chatInput.errors.locationSelectionCancelled'), 'info');
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
    openMapPicker,
    clearSelectedLocation,
    isPerformingOcr: ocrMutation.isPending,
    selectedLocation,
  };
}
