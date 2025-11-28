
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryDto } from '@/types/memory';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { DetectedFace, GenerateStoryCommand, PhotoAnalysisPersonDto } from '@/types';
import { useMemberStoryStore } from '@/stores/memberStory.store';

interface UseMemberStoryFormOptions {
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null;
  familyId?: string;
  updateModelValue: (payload: Partial<MemoryDto>) => void;
  onStoryGenerated: (payload: { story: string | null; title: string | null }) => void;
}

export function useMemberStoryForm(options: UseMemberStoryFormOptions) {
  const { modelValue, readonly, familyId, updateModelValue, onStoryGenerated } = options;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const memberStoryStore = useMemberStoryStore();

  const showSelectMemberDialog = ref(false);
  const faceToLabel = ref<DetectedFace | null>(null);
  const selectedTargetMemberFaceId = ref<string | null>(null);
  const uploadedImageUrl = ref<string | null>(null);

  const aiPerspectiveSuggestions = ref([
    { value: 'firstPerson', text: t('memory.create.perspective.firstPerson') },
    { value: 'neutralPersonal', text: t('memory.create.perspective.neutralPersonal') },
    { value: 'fullyNeutral', text: t('memory.create.perspective.fullyNeutral') },
  ]);

  const storyStyles = ref([
    { value: 'narrative', text: t('memory.create.storyStyle.narrative') },
    { value: 'descriptive', text: t('memory.create.storyStyle.descriptive') },
    { value: 'reflective', text: t('memory.create.storyStyle.reflective') },
    { value: 'journalistic', text: t('memory.create.storyStyle.journalistic') },
    { value: 'poetic', text: t('memory.create.storyStyle.poetic') },
  ]);

  const generatedStory = ref<string | null>(null);
  const generatedTitle = ref<string | null>(null);
  const generatingStory = ref(false);
  const storyEditorValid = ref(true); // TODO: Implement validation logic
  const hasUploadedImage = computed(() => !!uploadedImageUrl.value);
  const isLoading = computed(() => memberStoryStore.faceRecognition.loading);

  const canGenerateStory = computed(() => {
    const hasMinRawInput = modelValue.rawInput && modelValue.rawInput.length >= 10;
    const hasDetectedFaces = memberStoryStore.faceRecognition.detectedFaces.length > 0;
    const hasMemberSelected = modelValue.memberId;
    return (hasMinRawInput || hasDetectedFaces) && hasMemberSelected;
  });

  const generateStory = async () => {
    if (!canGenerateStory.value) return;

    if (!modelValue.memberId || modelValue.memberId === '00000000-0000-0000-0000-000000000000') {
      showSnackbar(t('memory.errors.memberIdRequired'), 'error');
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
      memberId: modelValue.memberId,
      resizedImageUrl: memberStoryStore.faceRecognition.resizedImageUrl,
      rawText: modelValue.rawInput,
      style: modelValue.storyStyle,
      photoPersons: photoPersonsPayload,
      perspective: modelValue.perspective,
    } as GenerateStoryCommand;

    const result = await memberStoryStore.generateStory(requestPayload);
    if (result.ok) {
      generatedStory.value = result.value.story;
      generatedTitle.value = result.value.title;
      updateModelValue({ story: generatedStory.value, title: generatedTitle.value });
      onStoryGenerated({ story: generatedStory.value, title: generatedTitle.value });
    } else {
      showSnackbar(result.error?.message || t('memory.errors.storyGenerationFailed'), 'error');
    }
    generatingStory.value = false;
  };

  watch(() => memberStoryStore.faceRecognition.error, (newError) => {
    if (newError) {
      showSnackbar(newError, 'error');
    }
  });

  watch(selectedTargetMemberFaceId, (newId) => {
    updateModelValue({ targetFaceId: newId ?? undefined });
    if (newId) {
      const selectedFace = modelValue.faces?.find(face => face.id === newId);
      if (selectedFace && selectedFace.memberId) {
        updateModelValue({ memberId: selectedFace.memberId, memberName: selectedFace.memberName });
      } else {
        updateModelValue({ memberId: null, memberName: null });
      }
    } else {
      updateModelValue({ memberId: null, memberName: null });
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
      uploadedImageUrl.value = URL.createObjectURL(uploadedFile);
      await memberStoryStore.detectFaces(uploadedFile, true);
      try {
        const img = await loadImage(uploadedFile);
        updateModelValue({ photoUrl: uploadedImageUrl.value, photo: uploadedImageUrl.value, imageSize: `${img.width}x${img.height}` });
      } catch (e) {
        console.error('Failed to load image for dimensions:', e);
        updateModelValue({ imageSize: undefined });
      }


      if (memberStoryStore.faceRecognition.detectedFaces.length > 0) {
        updateModelValue({
          faces: memberStoryStore.faceRecognition.detectedFaces,
        });

        if (memberStoryStore.faceRecognition.detectedFaces.length > 0) {
          selectedTargetMemberFaceId.value = memberStoryStore.faceRecognition.detectedFaces[0].id;
        }

      } else if (!isLoading.value && uploadedImageUrl.value && memberStoryStore.faceRecognition.detectedFaces.length === 0) {
        showSnackbar(t('face.recognition.noFacesDetected'), 'info');
        updateModelValue({
          faces: [],
          photo: undefined,
          imageSize: undefined,
          exifData: undefined,
        });
        selectedTargetMemberFaceId.value = null;
      }

    } else {
      memberStoryStore.resetFaceRecognitionState();
      uploadedImageUrl.value = null; // Clear local image URL
      updateModelValue({
        faces: [],
        photoUrl: undefined,
        photo: undefined,
        imageSize: undefined,
        exifData: undefined,
      });
      selectedTargetMemberFaceId.value = null;
    }
  };

  const openSelectMemberDialog = (faceId: string) => {
    const face = modelValue.faces?.find(f => f.id === faceId);
    if (face) {
      faceToLabel.value = face;
      showSelectMemberDialog.value = true;
    }
  };

  const handleLabelFaceAndCloseDialog = (updatedFace: DetectedFace) => {
    const updatedFaces = modelValue.faces?.map(face =>
      face.id === updatedFace.id ? updatedFace : face
    ) || [];
    updateModelValue({ faces: updatedFaces });
    showSelectMemberDialog.value = false;
    faceToLabel.value = null;
    if (selectedTargetMemberFaceId.value === updatedFace.id) {
      updateModelValue({ memberId: updatedFace.memberId, memberName: updatedFace.memberName });
    }
  };

  const handleRemoveFace = (faceId: string) => {
    const updatedFaces = modelValue.faces?.filter(face => face.id !== faceId) || [];
    updateModelValue({ faces: updatedFaces });
    if (selectedTargetMemberFaceId.value === faceId) {
      selectedTargetMemberFaceId.value = null;
    }
  };

  return {
    // State
    showSelectMemberDialog,
    faceToLabel,
    selectedTargetMemberFaceId,
    uploadedImageUrl,
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