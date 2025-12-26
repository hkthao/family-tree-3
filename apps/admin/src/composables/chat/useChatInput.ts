import { ref, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError, ImageUploadResultDto } from '@/types';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useMutation } from '@tanstack/vue-query';
import { useMapLocationDrawerStore } from '@/stores/mapLocationDrawer.store';
import { v4 as uuidv4 } from 'uuid'; // Import uuid for unique IDs

const MAX_FILE_COUNT = 3;
const MAX_FILE_SIZE_MB = 10;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;

interface LocationData {
  latitude: number;
  longitude: number;
  address?: string;
}

interface SelectedLocation extends LocationData {
  source: 'current' | 'map';
}

export interface UploadedFile {
  id: string; // Unique ID for the file chip
  name: string;
  url: string;
  deleteUrl?: string; // Optional delete URL if provided by the backend
  type: string; // Mime type or file extension
}

interface UseChatInputProps {
  modelValue?: string;
  disabled: boolean;
}

interface UseChatInputEmits {
  (event: 'update:modelValue', value: string): void;
  (event: 'sendMessage', attachments?: UploadedFile[]): void; // Updated to include attachments
  (event: 'addAttachment', type: 'image' | 'pdf'): void; // Keep this for now, might be removed later if addImagePdf handles everything
  (event: 'addLocation', location: LocationData): void;
}

export function useChatInput(props: UseChatInputProps, emit: UseChatInputEmits) {
  const { modelValue, disabled } = toRefs(props);
  const { t } = useI18n();
  const { chat: chatService } = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const mapDrawerStore = useMapLocationDrawerStore();

  const selectedLocation = ref<SelectedLocation | null>(null);
  const uploadedFiles = ref<UploadedFile[]>([]); // New state for uploaded files

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
    if ((currentModelValue.trim() || selectedLocation.value || uploadedFiles.value.length > 0) && !disabled.value) {
      if (selectedLocation.value) {
        emit('addLocation', selectedLocation.value);
        clearSelectedLocation();
      }
      emit('sendMessage', uploadedFiles.value); // Pass uploaded files
      uploadedFiles.value = []; // Clear uploaded files after sending message
    }
  };

  const removeUploadedFile = (id: string) => {
    uploadedFiles.value = uploadedFiles.value.filter(file => file.id !== id);
  };

  const uploadFileMutation = useMutation({
    mutationFn: (file: File) => chatService.uploadFile(file),
    onSuccess: (result) => {
      if (result.ok) {
        const uploadedFileDto: ImageUploadResultDto = result.value;
        uploadedFiles.value.push({
          id: uuidv4(), // Generate a unique ID for the chip
          name: uploadedFileDto.title || 'Untitled File',
          url: uploadedFileDto.url || '',
          deleteUrl: uploadedFileDto.deleteUrl,
          type: uploadedFileDto.mimeType || 'application/octet-stream',
        });
        showSnackbar(t('chatInput.uploadSuccess', { name: uploadedFileDto.title }), 'success');
      } else {
        const apiError = result.error as ApiError;
        showSnackbar(apiError.message || t('chatInput.uploadFailed'), 'error');
      }
    },
    onError: (error) => {
      console.error('File upload failed:', error);
      showSnackbar(t('chatInput.uploadFailed'), 'error');
    },
  });

  const addImagePdf = (file: File) => {
    // Validate file count
    if (uploadedFiles.value.length >= MAX_FILE_COUNT) {
      showSnackbar(t('chatInput.errors.maxFileCount', { count: MAX_FILE_COUNT }), 'warning');
      return;
    }

    // Validate file size
    if (file.size > MAX_FILE_SIZE_BYTES) {
      showSnackbar(t('chatInput.errors.maxFileSize', { size: MAX_FILE_SIZE_MB }), 'warning');
      return;
    }

    uploadFileMutation.mutate(file);
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
    isPerformingOcr: uploadFileMutation.isPending, // Renamed from ocrMutation.isPending
    selectedLocation,
    uploadedFiles, // Expose uploadedFiles
    removeUploadedFile, // Expose removeUploadedFile
    isUploadingFile: uploadFileMutation.isPending, // Expose isUploadingFile
  };
}
