import { ref, watch, computed, type ComputedRef } from 'vue';
import type { MemberStoryDto } from '@/types/memberStory';
import { useGlobalSnackbar } from '../ui/useGlobalSnackbar';
import type { DetectedFace, FaceDetectionRessult } from '@/types'; // Added FaceDetectionRessult
import { useDetectFacesMutation } from '@/composables/face'; // New import
import { useI18n } from 'vue-i18n'; // Added import

interface UseMemberStoryFormOptions {
  modelValue: ComputedRef<MemberStoryDto>;
  readonly?: boolean;
  memberId?: string | null;
  familyId?: string;
  updateModelValue: (payload: Partial<MemberStoryDto>) => void;
}

export function useMemberStoryForm(options: UseMemberStoryFormOptions) {
  const { modelValue, readonly, updateModelValue } = options;
  const { t } = useI18n(); // Initialized i18n
  const { showSnackbar } = useGlobalSnackbar();

  const { mutateAsync: detectFaces, isPending: isDetectingFaces, error: detectFacesError } = useDetectFacesMutation();

  const showSelectMemberDialog = ref(false);
  const faceToLabel = ref<DetectedFace | null>(null);

  const hasUploadedImage = computed(() => {
    return !!modelValue.value.temporaryOriginalImageUrl || (modelValue.value.memberStoryImages && modelValue.value.memberStoryImages.length > 0);
  });

  const isLoading = computed(() => {
    // Combine loading states for face detection with other potential loadings
    return isDetectingFaces.value;
  });

  // Watch for face detection errors
  watch(detectFacesError, (newError) => {
    if (newError) {
      showSnackbar(newError.message || t('memberStory.faceRecognition.error'), 'error');
    }
  });

  const loadImage = (file: File): Promise<HTMLImageElement> => {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = URL.createObjectURL(file);
    });
  };

  const handleFileUpload = async (file: File | File[] | null) => {
    if (readonly) return;
    let uploadedFile: File | null = null;
    if (file instanceof File) {
      uploadedFile = file;
    } else if (Array.isArray(file) && file.length > 0) {
      uploadedFile = file[0];
    }
    if (uploadedFile) {
      // Clear previous face recognition state / detected faces
      updateModelValue({
        detectedFaces: [],
        temporaryOriginalImageUrl: undefined,
        temporaryResizedImageUrl: undefined,
        imageSize: undefined,
        exifData: undefined,
      });

      const temporaryUrl = URL.createObjectURL(uploadedFile);

      try {
        // Call the detectFaces mutation
        const result: FaceDetectionRessult = await detectFaces({
          imageFile: uploadedFile,
          familyId: options.familyId!, // familyId is required for face detection
          resizeImageForAnalysis: true,
        });

        const img = await loadImage(uploadedFile);
        updateModelValue({
          temporaryOriginalImageUrl: temporaryUrl,
          temporaryResizedImageUrl: result.resizedImageUrl,
          imageSize: `${img.width}x${img.height}`,
          detectedFaces: result.detectedFaces, // Update detectedFaces from mutation result
        });
      } catch (e) {
        console.error('Error during face detection or image loading:', e);
        showSnackbar((e as Error).message || t('memberStory.faceRecognition.error'), 'error');
        updateModelValue({ imageSize: undefined });
      }
    } else {
      updateModelValue({
        detectedFaces: [], // Clear detectedFaces
        temporaryOriginalImageUrl: undefined,
        temporaryResizedImageUrl: undefined,
        imageSize: undefined,
        exifData: undefined,
      });
    }
  };

  const openSelectMemberDialog = (face: DetectedFace) => {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  };

  const handleLabelFaceAndCloseDialog = (updatedFace: DetectedFace) => {
    const currentFaces = modelValue.value.detectedFaces ? [...modelValue.value.detectedFaces] : [];
    const index = currentFaces.findIndex(face => face.id === updatedFace.id);
    if (index !== -1) {
      currentFaces[index] = { ...updatedFace };
    }
    updateModelValue({ detectedFaces: currentFaces });
    showSelectMemberDialog.value = false;
    faceToLabel.value = null;
  };

  const handleRemoveFace = (faceId: string) => {
    const updatedFaces = modelValue.value.detectedFaces?.filter(face => face.id !== faceId) || [];
    updateModelValue({ detectedFaces: updatedFaces });
  };

  return {
    showSelectMemberDialog,
    faceToLabel,
    hasUploadedImage,
    isLoading,
    handleFileUpload,
    openSelectMemberDialog,
    handleLabelFaceAndCloseDialog,
    handleRemoveFace,
  };
}