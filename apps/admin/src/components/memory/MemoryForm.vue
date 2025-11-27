<template>
  <v-container>
    <!-- Photo Upload Input -->
    <v-row>
      <v-col v-if="isLoading" cols="12">
        <v-progress-linear indeterminate color="primary"></v-progress-linear>
      </v-col>
      <v-col cols="12">
        <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" :readonly="readonly" />
      </v-col>
    </v-row>
    <!-- Face Detection and Selection -->
    <v-row>
      <v-col cols="12">
        <div v-if="hasUploadedImage && !isLoading">
          <div v-if="internalMemory.faces && internalMemory.faces.length > 0">
            <FaceBoundingBoxViewer :image-src="uploadedImageUrl!" :faces="internalMemory.faces" selectable
              @face-selected="openSelectMemberDialog" />
            <FaceDetectionSidebar :faces="internalMemory.faces" @face-selected="openSelectMemberDialog"
              @remove-face="handleRemoveFace" />
            <h4>{{ t('memory.create.selectTargetMember') }}</h4>
            <v-chip-group v-model="selectedTargetMemberFaceId" mandatory column>
              <MemberFaceChip v-for="face in internalMemory.faces" :key="face.id" :face="face" :value="face.id" />
            </v-chip-group>
          </div>
          <v-alert v-else type="info">{{ t('face.recognition.noFacesDetected') }}</v-alert>
        </div>
        <v-alert v-else type="info">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
      </v-col>
    </v-row>

    <!-- Raw Input & Story Style -->
    <v-row v-if="hasUploadedImage && !isLoading">
      <v-col cols="12">
        <h4>{{ t('memory.create.rawInputPlaceholder') }}</h4>
        <v-textarea class="mt-4" :model-value="internalMemory.rawInput" :rows="2"
          @update:model-value="(newValue) => updateMemory({ rawInput: newValue })"
          :label="t('memory.create.rawInputPlaceholder')" :readonly="readonly" auto-grow></v-textarea>
        <StoryStyleInput :model-value="internalMemory.storyStyle"
          @update:model-value="(newValue) => updateMemory({ storyStyle: newValue })" :readonly="readonly" />
      </v-col>
    </v-row>

    <!-- Perspective -->
    <v-row v-if="hasUploadedImage && !isLoading">
      <v-col cols="12">
        <h4>{{ t('memory.create.perspective.question') }}</h4>
        <v-chip-group :model-value="internalMemory.perspective"
          @update:model-value="(newValue) => updateMemory({ perspective: newValue })" color="primary" mandatory column
          :disabled="readonly">
          <v-chip :value="aiPerspectiveSuggestions[0].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[0].text }}
          </v-chip>
          <v-chip :value="aiPerspectiveSuggestions[1].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[1].text }}
          </v-chip>
          <v-chip :value="aiPerspectiveSuggestions[2].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[2].text }}
          </v-chip>
        </v-chip-group>
      </v-col>
    </v-row>

    <!-- Title and Story -->
    <v-row v-if="hasUploadedImage && !isLoading">
      <v-col cols="12">
        <h4 class="mb-2">{{ t('memory.create.step4.reviewEdit') }}</h4>
        <v-text-field v-model="internalMemory.title" :label="t('memory.storyEditor.title')" outlined
          class="mb-4"></v-text-field>
        <v-textarea v-model="internalMemory.story" :label="t('memory.storyEditor.storyContent')" outlined
          auto-grow></v-textarea>
      </v-col>
    </v-row>

    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="props.familyId" />
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryDto } from '@/types/memory';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useMemoryStore } from '@/stores/memory.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import type { DetectedFace, Member } from '@/types';
import MemberFaceChip from '../common/MemberFaceChip.vue';
import StoryStyleInput from './StoryStyleInput.vue';
import type { PhotoAnalysisPersonDto } from '@/types/ai';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null;
  familyId?: string;
}>();
const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles', 'story-generated']);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const memoryStore = useMemoryStore();
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);
const showSelectMemberDialog = ref(false);
const faceToLabel = ref<DetectedFace | null>(null);
const selectedTargetMemberFaceId = ref<string | null>(null);
const uploadedImageUrl = ref<string | null>(null);
const aiPerspectiveSuggestions = ref([
  { value: 'firstPerson', text: t('memory.create.perspective.firstPerson') },
  { value: 'neutralPersonal', text: t('memory.create.perspective.neutralPersonal') },
  { value: 'fullyNeutral', text: t('memory.create.perspective.fullyNeutral') },
]);
const generatedStory = ref<string | null>(null);
const generatedTitle = ref<string | null>(null);
const generatingStory = ref(false);
const storyEditorValid = ref(true);
const hasUploadedImage = computed(() => !!uploadedImageUrl.value);
const isLoading = computed(() => memoryStore.faceRecognition.loading);
const canGenerateStory = computed(() => {
  return (internalMemory.value.rawInput && internalMemory.value.rawInput.length >= 10) ||
    (memoryStore.faceRecognition.uploadedImageId && memoryStore.faceRecognition.detectedFaces.length > 0);
});

const updateMemory = (payload: Partial<MemoryDto>) => {
  emit('update:modelValue', { ...internalMemory.value, ...payload });
};

const generateStory = async () => {
  if (!canGenerateStory.value) return;

  generatingStory.value = true;

  const photoPersonsPayload: PhotoAnalysisPersonDto[] = memoryStore.faceRecognition.detectedFaces.map(face => ({
    id: face.id,
    memberId: face.memberId,
    name: face.memberName,
    emotion: face.emotion,
    confidence: face.emotionConfidence,
    relationPrompt: face.relationPrompt,
  }));

  const requestPayload = {
    memberId: internalMemory.value.memberId,
    resizedImageUrl: memoryStore.faceRecognition.resizedImageUrl,
    rawText: internalMemory.value.rawInput,
    style: internalMemory.value.storyStyle,
    maxWords: 500,
    photoSummary: internalMemory.value.photoAnalysisResult?.summary,
    photoScene: internalMemory.value.photoAnalysisResult?.scene,
    photoEventAnalysis: internalMemory.value.photoAnalysisResult?.event,
    photoEmotionAnalysis: internalMemory.value.photoAnalysisResult?.emotion,
    photoYearEstimate: internalMemory.value.photoAnalysisResult?.yearEstimate,
    photoObjects: internalMemory.value.photoAnalysisResult?.objects,
    photoPersons: photoPersonsPayload,
    perspective: internalMemory.value.perspective,
    event: internalMemory.value.eventSuggestion,
    customEventDescription: internalMemory.value.customEventDescription,
    emotionContexts: internalMemory.value.emotionContextTags,
  };

  const result = await memoryStore.generateStory(requestPayload);
  if (result.ok) {
    generatedStory.value = result.value.story;
    generatedTitle.value = result.value.title;
    internalMemory.value.story = generatedStory.value;
    internalMemory.value.title = generatedTitle.value;
    emit('story-generated', { story: generatedStory.value, title: generatedTitle.value });
  } else {
    showSnackbar(result.error?.message || t('memory.errors.storyGenerationFailed'), 'error');
  }
  generatingStory.value = false;
};

const internalMemory = computed<MemoryDto>({
  get: () => props.modelValue,
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

watch(() => memoryStore.faceRecognition.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

watch(selectedTargetMemberFaceId, (newId) => {
  internalMemory.value.targetFaceId = newId ?? undefined;
  if (newId) {
    const selectedFace = internalMemory.value.faces?.find(face => face.id === newId);
    if (selectedFace && selectedFace.memberId) {
      internalMemory.value.memberId = selectedFace.memberId;
    } else {
      internalMemory.value.memberId = null;
    }
  } else {
    internalMemory.value.memberId = null;
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
  if (props.readonly) return;
  let uploadedFile: File | null = null;
  if (file instanceof File) {
    uploadedFile = file;
  } else if (Array.isArray(file) && file.length > 0) {
    uploadedFile = file[0];
  }
  if (uploadedFile) {
    uploadedImageUrl.value = URL.createObjectURL(uploadedFile);
    await memoryStore.detectFaces(uploadedFile, true);
    try {
      const img = await loadImage(uploadedFile);
      updateMemory({ imageSize: `${img.width}x${img.height}` });
    } catch (e) {
      console.error('Failed to load image for dimensions:', e);
      updateMemory({ imageSize: undefined });
    }


    if (memoryStore.faceRecognition.detectedFaces.length > 0) {
      updateMemory({
        faces: memoryStore.faceRecognition.detectedFaces,
        photoUrl: uploadedImageUrl.value,
        photo: uploadedImageUrl.value,
      });

      if (memoryStore.faceRecognition.detectedFaces.length > 0) {
        selectedTargetMemberFaceId.value = memoryStore.faceRecognition.detectedFaces[0].id;
      }

    } else if (!isLoading && uploadedImageUrl.value && memoryStore.faceRecognition.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
      updateMemory({
        faces: [],
        photo: undefined,
        imageSize: undefined,
        exifData: undefined,
      });
      selectedTargetMemberFaceId.value = null;
    }

  } else {
    memoryStore.resetFaceRecognitionState();
    uploadedImageUrl.value = null; // Clear local image URL
    updateMemory({
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
  const face = internalMemory.value.faces?.find(f => f.id === faceId);
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const handleLabelFaceAndCloseDialog = (faceId: string, memberDetails: Member) => {
  const updatedFaces = internalMemory.value.faces?.map(face =>
    face.id === faceId ? { ...face, memberId: memberDetails.id, memberName: memberDetails.fullName } : face
  ) || [];
  updateMemory({ faces: updatedFaces });
  showSelectMemberDialog.value = false;
  faceToLabel.value = null;
  if (selectedTargetMemberFaceId.value === faceId) {
    internalMemory.value.memberId = memberDetails.id;
  }
};

const handleRemoveFace = (faceId: string) => {
  const updatedFaces = internalMemory.value.faces?.filter(face => face.id !== faceId) || [];
  updateMemory({ faces: updatedFaces });
  if (selectedTargetMemberFaceId.value === faceId) {
    selectedTargetMemberFaceId.value = null;
  }
};

defineExpose({
  isValid: computed(() => !isLoading.value),
  memoryFaceStore: memoryStore.faceRecognition,
  faceUploadInputRef,
  selectedTargetMemberFaceId,
  generateStory,
  generatedStory,
  generatedTitle,
  storyEditorValid,
});
</script>

<style scoped>
/* Remove stepper-header styles as stepper is removed */
</style>