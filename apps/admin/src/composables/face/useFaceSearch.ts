import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useDetectFacesMutation } from '@/composables/member-face';
import type { DetectedFace } from '@/types';

export function useFaceSearch() {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const { mutate: detectFaces, isPending: isDetectingFaces, error: detectError } = useDetectFacesMutation();

  const selectedFamilyId = ref<string | undefined>(undefined);
  const uploadedImage = ref<string | null | undefined>(undefined);
  const detectedFaces = ref<DetectedFace[]>([]);
  const originalImageUrl = ref<string | null>(null);

  watch(detectError, (newError) => {
    if (newError) {
      showSnackbar(newError.message, 'error');
    }
  });

  const resetState = () => {
    uploadedImage.value = null;
    detectedFaces.value = [];
    originalImageUrl.value = null;
  };

  const handleFileUpload = async (file: File | null) => {
    resetState();
    if (!file) {
      return;
    }
    if (!selectedFamilyId.value) {
      showSnackbar(t('face.selectFamilyToUpload'), 'warning');
      return;
    }
    detectFaces({ imageFile: file, familyId: selectedFamilyId.value, resize: true }, {
      onSuccess: (data) => {
        uploadedImage.value = data.originalImageBase64;
        detectedFaces.value = data.detectedFaces;
        originalImageUrl.value = data.originalImageUrl;
      },
      onError: (error) => {
        showSnackbar(error.message, 'error');
      },
    });
  };

  return {
    selectedFamilyId,
    uploadedImage,
    detectedFaces,
    originalImageUrl,
    isDetectingFaces,
    handleFileUpload,
    resetState,
    t, // Expose t for use in component, as it's often used in templates
  };
}