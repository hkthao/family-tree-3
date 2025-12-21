import { ref, watch } from 'vue';
import { useI18n, type Composer } from 'vue-i18n';
import { useGlobalSnackbar, type UseGlobalSnackbarReturn } from '@/composables/ui/useGlobalSnackbar';
import { useDetectFacesMutation, type UseDetectFacesMutationReturn } from '@/composables/face/mutations/useDetectFacesMutation';
import type { DetectedFace } from '@/types';

interface UseFaceSearchDeps {
  useI18n: () => Composer;
  useGlobalSnackbar: () => UseGlobalSnackbarReturn;
  useDetectFacesMutation: () => UseDetectFacesMutationReturn;
  FileReader: typeof FileReader;
}

const defaultDeps: UseFaceSearchDeps = {
  useI18n,
  useGlobalSnackbar,
  useDetectFacesMutation,
  FileReader,
};

export function useFaceSearch(
  deps: UseFaceSearchDeps = defaultDeps,
) {
  const { useI18n: injectedUseI18n, useGlobalSnackbar: injectedUseGlobalSnackbar, useDetectFacesMutation: injectedUseDetectFacesMutation, FileReader: InjectedFileReader } = deps;
  const { t } = injectedUseI18n();
  const { showSnackbar } = injectedUseGlobalSnackbar();

  const { mutate: detectFaces, isPending: isDetectingFaces, error: detectError } = injectedUseDetectFacesMutation();

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

    // Immediately create a local URL for display
    const reader = new InjectedFileReader();
    reader.onload = (e) => {
      uploadedImage.value = e.target?.result as string;
    };
    reader.readAsDataURL(file);

    detectFaces({ imageFile: file, familyId: selectedFamilyId.value, resize: true }, {
      onSuccess: (data: { detectedFaces: DetectedFace[]; originalImageUrl: string | null }) => {
        // uploadedImage.value is now set from local blob
        detectedFaces.value = data.detectedFaces;
        originalImageUrl.value = data.originalImageUrl;
      },
      onError: (error: Error) => {
        showSnackbar(error.message, 'error');
        uploadedImage.value = null; // Clear image on error
        detectedFaces.value = []; // Ensure faces are cleared on error
      },
    });
  };

  return {
    state: {
      selectedFamilyId,
      uploadedImage,
      detectedFaces,
      originalImageUrl,
      isDetectingFaces,
      t, // Expose t for use in component, as it's often used in templates
    },
    actions: {
      handleFileUpload,
      resetState,
    },
  };
}