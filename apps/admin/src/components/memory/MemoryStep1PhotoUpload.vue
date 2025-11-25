<template>
  <div>
    <v-row>
      <v-col v-if="memoryStore.faceRecognition.loading" cols="12">
        <v-progress-linear indeterminate color="primary"></v-progress-linear>
      </v-col>
      <v-col cols="12">
        <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" :readonly="readonly" />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <div v-if="memoryStore.faceRecognition.uploadedImage && memoryStore.faceRecognition.detectedFaces.length > 0">
          <FaceBoundingBoxViewer :image-src="memoryStore.faceRecognition.uploadedImage" :faces="memoryStore.faceRecognition.detectedFaces"
            selectable @face-selected="openSelectMemberDialog" />
          <FaceDetectionSidebar :faces="memoryStore.faceRecognition.detectedFaces" @face-selected="openSelectMemberDialog"
            @remove-face="handleRemoveFace" />

          <!-- Main Character Selection -->
          <v-card flat>
            <v-card-title>{{ t('memory.create.selectMainCharacter') }}</v-card-title>
            <v-card-text>
              <v-chip-group v-model="selectedMainCharacterFaceId" mandatory column>
                <v-chip v-for="face in memoryStore.faceRecognition.detectedFaces" :key="face.id" :value="face.id"
                  variant="outlined" color="primary" filter>
                  <v-avatar left>
                    <v-img :src="getFaceThumbnailSrc(face)" alt="Face"></v-img>
                  </v-avatar>
                  <span class="ml-2">{{ face.memberName || t('common.unknown') }}</span>
                  <v-chip v-if="face.emotion" size="x-small" class="ml-2">{{ face.emotion }}</v-chip>
                </v-chip>
              </v-chip-group>
            </v-card-text>
          </v-card>
        </div>
        <v-alert v-else-if="!memoryStore.faceRecognition.loading && !memoryStore.faceRecognition.uploadedImage" type="info">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert
          v-else-if="!memoryStore.faceRecognition.loading && memoryStore.faceRecognition.uploadedImage && memoryStore.faceRecognition.detectedFaces.length === 0"
          type="info">{{ t('face.recognition.noFacesDetected') }}</v-alert>
      </v-col>
    </v-row>
    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="props.familyId" />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store'; // Use the main memory store
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import type { MemoryDto } from '@/types/memory';
import type { DetectedFace, Member } from '@/types';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  familyId?: string;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const memoryStore = useMemoryStore(); // Initialize the main memory store
const { showSnackbar } = useGlobalSnackbar();
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);
const showSelectMemberDialog = ref(false);
const faceToLabel = ref<DetectedFace | null>(null);
const selectedMainCharacterFaceId = ref<string | null>(null); // New ref for main character selection

const internalMemory = computed<MemoryDto>({
  get: () => props.modelValue,
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

watch(() => memoryStore.faceRecognition.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

// Watch for changes in selectedMainCharacterFaceId to update internalMemory.memberId
watch(selectedMainCharacterFaceId, (newId) => {
  if (newId) {
    const selectedFace = memoryStore.faceRecognition.detectedFaces.find(face => face.id === newId);
    if (selectedFace && selectedFace.memberId) {
      internalMemory.value.memberId = selectedFace.memberId;
    } else {
      internalMemory.value.memberId = null; // Clear if no memberId on selected face
    }
  } else {
    internalMemory.value.memberId = null; // Clear if nothing selected
  }
});

const handleFileUpload = async (file: File | File[] | null) => {
  if (props.readonly) return;

        if (file instanceof File) {
          await memoryStore.detectFaces(file);
          if (memoryStore.faceRecognition.detectedFaces.length > 0) {
            internalMemory.value.photoAnalysisId = 'generated_id';
            internalMemory.value.faces = memoryStore.faceRecognition.detectedFaces;
            internalMemory.value.photoUrl = memoryStore.faceRecognition.uploadedImage;
            internalMemory.value.photo = memoryStore.faceRecognition.uploadedImage; // Assign to the new 'photo' property
            // Automatically select the first face as main character if any faces are detected
            if (memoryStore.faceRecognition.detectedFaces.length > 0) {
              selectedMainCharacterFaceId.value = memoryStore.faceRecognition.detectedFaces[0].id;
            }
          } else if (!memoryStore.faceRecognition.loading && memoryStore.faceRecognition.uploadedImage && memoryStore.faceRecognition.detectedFaces.length === 0) {
            showSnackbar(t('face.recognition.noFacesDetected'), 'info');
            internalMemory.value.faces = []; // Clear faces if none detected
            selectedMainCharacterFaceId.value = null;
            internalMemory.value.photo = undefined; // Clear photo if no faces detected
          }
        } else if (Array.isArray(file) && file.length > 0) {
          await memoryStore.detectFaces(file[0]);
          if (memoryStore.faceRecognition.detectedFaces.length > 0) {
            internalMemory.value.photoAnalysisId = 'generated_id';
            internalMemory.value.faces = memoryStore.faceRecognition.detectedFaces;
            internalMemory.value.photoUrl = memoryStore.faceRecognition.uploadedImage;
            internalMemory.value.photo = memoryStore.faceRecognition.uploadedImage; // Assign to the new 'photo' property
            if (memoryStore.faceRecognition.detectedFaces.length > 0) {
              selectedMainCharacterFaceId.value = memoryStore.faceRecognition.detectedFaces[0].id;
            }
          } else if (!memoryStore.faceRecognition.loading && memoryStore.faceRecognition.uploadedImage && memoryStore.faceRecognition.detectedFaces.length === 0) {
            showSnackbar(t('face.recognition.noFacesDetected'), 'info');
            internalMemory.value.faces = []; // Clear faces if none detected
            selectedMainCharacterFaceId.value = null;
            internalMemory.value.photo = undefined; // Clear photo if no faces detected
          }
        }
        else {
          memoryStore.resetFaceRecognitionState();
          internalMemory.value.photoAnalysisId = undefined;
          internalMemory.value.faces = [];
          internalMemory.value.photoUrl = undefined;
          internalMemory.value.photo = undefined; // Clear the 'photo' property
          selectedMainCharacterFaceId.value = null;
        }};

const openSelectMemberDialog = (faceId: string) => {
  memoryStore.selectFace(faceId);
  const face = memoryStore.currentSelectedFace;
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const handleLabelFaceAndCloseDialog = (faceId: string, memberDetails: Member) => {
  memoryStore.labelFace(faceId, memberDetails.id, memberDetails);
  showSelectMemberDialog.value = false;
  faceToLabel.value = null;
  // If the labeled face is currently selected as main character, update internalMemory.memberId
  if (selectedMainCharacterFaceId.value === faceId) {
    internalMemory.value.memberId = memberDetails.id;
  }
};

const handleRemoveFace = (faceId: string) => {
  memoryStore.removeFace(faceId);
  // If the removed face was the main character, clear selection
  if (selectedMainCharacterFaceId.value === faceId) {
    selectedMainCharacterFaceId.value = null;
  }
};

const canSaveLabels = computed(() => {
  return memoryStore.faceRecognition.detectedFaces.some(
    (face) =>
      face.memberId &&
      (face.originalMemberId === null ||
        face.originalMemberId === undefined ||
        face.memberId !== face.originalMemberId),
  );
});

const saveLabels = async () => {
  const result = await memoryStore.saveFaceLabels();
  if (result.ok) {
    showSnackbar(t('face.recognition.saveSuccess'), 'success');
  }
};

const getFaceThumbnailSrc = (face: DetectedFace) => {
  if (face.thumbnail) {
    return `data:image/jpeg;base64,${face.thumbnail}`;
  }
  return '';
};

defineExpose({
  isValid: computed(() => !memoryStore.faceRecognition.loading),
  memoryFaceStore: memoryStore.faceRecognition, // Expose memoryStore.faceRecognition instead of memoryFaceStore
  faceUploadInputRef,
  selectedMainCharacterFaceId, // Expose for potential validation in parent
});
</script>

<style scoped>
</style>