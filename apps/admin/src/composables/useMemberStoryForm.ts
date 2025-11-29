
import { ref, watch, computed, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { DetectedFace, GenerateStoryCommand, PhotoAnalysisPersonDto } from '@/types';
import { useMemberStoryStore } from '@/stores/memberStory.store';

interface UseMemberStoryFormOptions {
  modelValue: ComputedRef<MemberStoryDto>; // Changed from MemberStoryDto
  readonly?: boolean;
  memberId?: string | null;
  familyId?: string;
  updateModelValue: (payload: Partial<MemberStoryDto>) => void;
  onStoryGenerated: (payload: { story: string | null; title: string | null }) => void;
}

export function useMemberStoryForm(options: UseMemberStoryFormOptions) {
  const { modelValue, readonly, updateModelValue, onStoryGenerated } = options;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const memberStoryStore = useMemberStoryStore();

  const showSelectMemberDialog = ref(false);
  const faceToLabel = ref<DetectedFace | null>(null);

  const aiPerspectiveSuggestions = ref([
    { value: 'firstPerson', text: t('memberStory.create.perspective.firstPerson') },
    { value: 'neutralPersonal', text: t('memberStory.create.perspective.neutralPersonal') },
    { value: 'fullyNeutral', text: t('memberStory.create.perspective.fullyNeutral') },
  ]);

  const storyStyles = ref([
    { value: 'nostalgic', text: t('memberStory.style.nostalgic') },
    { value: 'warm', text: t('memberStory.style.warm') },
    { value: 'formal', text: t('memberStory.style.formal') },
    { value: 'folk', text: t('memberStory.style.folk') },
  ]);

  const generatedStory = ref<string | null>(null);
  const generatedTitle = ref<string | null>(null);
  const generatingStory = ref(false);
  const storyEditorValid = ref(true); // TODO: Implement validation logic
  const hasUploadedImage = computed(() => {
    return !!modelValue.value.originalImageUrl; // Updated to check resizedImageUrl
  });
  const isLoading = computed(() => memberStoryStore.faceRecognition.loading);

  const canGenerateStory = computed(() => {
    const hasMinRawInput = modelValue.value.rawInput && modelValue.value.rawInput.length >= 10;
    const hasDetectedFaces = modelValue.value.detectedFaces && modelValue.value.detectedFaces.length > 0; // Check detectedFaces
    const hasMemberSelected = modelValue.value.memberId;
    return (hasMinRawInput || hasDetectedFaces) && hasMemberSelected;
  });

  const generateStory = async () => {
    if (!canGenerateStory.value) return;

    if (!modelValue.value.memberId || modelValue.value.memberId === '00000000-0000-0000-0000-000000000000') {
      showSnackbar(t('memberStory.errors.memberIdRequired'), 'error');
      generatingStory.value = false;
      return;
    }

    generatingStory.value = true;

    const photoPersonsPayload: PhotoAnalysisPersonDto[] = memberStoryStore.faceRecognition.detectedFaces.map((face: DetectedFace) => ({
      id: face.id,
      memberId: face.memberId,
      name: face.memberName,
      emotion: face.emotion,
      confidence: face.emotionConfidence,
      relationPrompt: face.relationPrompt,
    }));

    const requestPayload = {
      memberId: modelValue.value.memberId,
      resizedImageUrl: memberStoryStore.faceRecognition.resizedImageUrl,
      rawText: modelValue.value.rawInput,
      style: modelValue.value.storyStyle,
      photoPersons: photoPersonsPayload,
      perspective: modelValue.value.perspective,
    } as GenerateStoryCommand;

    const result = await memberStoryStore.generateStory(requestPayload);
    if (result.ok) {
      generatedStory.value = result.value.story;
      generatedTitle.value = result.value.title;
      updateModelValue({ story: generatedStory.value, title: generatedTitle.value });
      onStoryGenerated({ story: generatedStory.value, title: generatedTitle.value });
    } else {
      showSnackbar(result.error?.message || t('memberStory.errors.storyGenerationFailed'), 'error');
    }
    generatingStory.value = false;
  };

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
      updateModelValue({ photo: URL.createObjectURL(uploadedFile) });

      await memberStoryStore.detectFaces(uploadedFile, true);
      try {
        const img = await loadImage(uploadedFile);
        updateModelValue({
          originalImageUrl: memberStoryStore.faceRecognition.originalImageUrl, // Get from store
          resizedImageUrl: memberStoryStore.faceRecognition.resizedImageUrl, // Get from store
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
        originalImageUrl: undefined,
        resizedImageUrl: undefined,
        photo: undefined,
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
    aiPerspectiveSuggestions,
    storyStyles,
    generatedStory,
    generatedTitle,
    generatingStory,
    storyEditorValid,
    hasUploadedImage,
    isLoading,
    canGenerateStory,
    // Methods
    generateStory,
    handleFileUpload,
    openSelectMemberDialog,
    handleLabelFaceAndCloseDialog,
    handleRemoveFace,
    // Expose for parent access if needed
    memberStoryStoreFaceRecognition: memberStoryStore.faceRecognition,
  };
}