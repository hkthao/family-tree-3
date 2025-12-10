
import { ref, watch, computed, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { DetectedFace } from '@/types';
import { useMemberStoryStore } from '@/stores/memberStory.store';

interface UseMemberStoryFormOptions {
  modelValue: ComputedRef<MemberStoryDto>;
  readonly?: boolean;
  memberId?: string | null;
  familyId?: string;
  updateModelValue: (payload: Partial<MemberStoryDto>) => void;
}

export function useMemberStoryForm(options: UseMemberStoryFormOptions) {
  const { modelValue, readonly, updateModelValue } = options;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const memberStoryStore = useMemberStoryStore();

  const showSelectMemberDialog = ref(false);
  const faceToLabel = ref<DetectedFace | null>(null);

  const hasUploadedImage = computed(() => {
    return !!modelValue.value.temporaryOriginalImageUrl || (modelValue.value.memberStoryImages && modelValue.value.memberStoryImages.length > 0);
  });
  const isLoading = computed(() => {
    return memberStoryStore.faceRecognition.loading ||
           memberStoryStore.add.loading ||
           memberStoryStore.update.loading ||
           memberStoryStore._delete.loading ||
           memberStoryStore.aiAnalysis.loading; // aiAnalysis.loading will still be true here, this needs fixing
  });

  watch(() => memberStoryStore.faceRecognition.error, (newError) => {
    if (newError) {
      showSnackbar(newError, 'error');
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
      // Clear previous face recognition state before new upload
      memberStoryStore.resetFaceRecognitionState(); 
      // Update photo for display to temporary URL
      const temporaryUrl = URL.createObjectURL(uploadedFile);

      await memberStoryStore.detectFaces(uploadedFile, modelValue.value.familyId!, true);
      try {
        const img = await loadImage(uploadedFile);
        updateModelValue({
          temporaryOriginalImageUrl: temporaryUrl,
          temporaryResizedImageUrl: memberStoryStore.faceRecognition.resizedImageUrl,
          imageSize: `${img.width}x${img.height}`,
          detectedFaces: memberStoryStore.faceRecognition.detectedFaces, // Update detectedFaces
        });
      } catch (e) {
        console.error('Failed to load image for dimensions:', e);
        updateModelValue({ imageSize: undefined });
      }
    } else {
      memberStoryStore.resetFaceRecognitionState();
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
    // Make a shallow copy of the detectedFaces array to ensure reactivity update
    const currentFaces = modelValue.value.detectedFaces ? [...modelValue.value.detectedFaces] : [];
    const index = currentFaces.findIndex(face => face.id === updatedFace.id);
    if (index !== -1) {
      // Create a new object for the updated face to ensure full reactivity update
      currentFaces[index] = { ...updatedFace };
    }
    updateModelValue({ detectedFaces: currentFaces }); // Update with the new array
    showSelectMemberDialog.value = false;
    faceToLabel.value = null;
  };

  const handleRemoveFace = (faceId: string) => {
    const updatedFaces = modelValue.value.detectedFaces?.filter(face => face.id !== faceId) || [];
    updateModelValue({ detectedFaces: updatedFaces });
  };

  return {
    // State
    showSelectMemberDialog,
    faceToLabel,
    hasUploadedImage,
    isLoading,
    // Methods
    handleFileUpload,
    openSelectMemberDialog,
    handleLabelFaceAndCloseDialog,
    handleRemoveFace,
    // Expose for parent access if needed
    memberStoryStoreFaceRecognition: memberStoryStore.faceRecognition,
  };
}