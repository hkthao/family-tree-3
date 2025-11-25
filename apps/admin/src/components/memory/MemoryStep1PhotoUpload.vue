<template>
  <div>
    <v-row>
      <v-col v-if="memoryFaceStore.loading" cols="12">
        <v-progress-linear indeterminate color="primary"></v-progress-linear>
      </v-col>
      <v-col cols="12">
        <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" :readonly="readonly" />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <div v-if="memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length > 0">
          <FaceBoundingBoxViewer :image-src="memoryFaceStore.uploadedImage" :faces="memoryFaceStore.detectedFaces" selectable
            @face-selected="openSelectMemberDialog" />
          <FaceDetectionSidebar :faces="memoryFaceStore.detectedFaces" @face-selected="openSelectMemberDialog"
            @remove-face="handleRemoveFace" />
        </div>
        <v-alert v-else-if="!memoryFaceStore.loading && !memoryFaceStore.uploadedImage" type="info">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!memoryFaceStore.loading && memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length === 0"
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
import { useMemoryFaceStore } from '@/stores/memoryFaceStore';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import type { MemoryDto } from '@/types/memory';
import type { DetectedFace, Member } from '@/types'; // Import Member type
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  familyId?: string; // Add familyId prop
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const memoryFaceStore = useMemoryFaceStore();
const { showSnackbar } = useGlobalSnackbar();
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);
const showSelectMemberDialog = ref(false); // Add for dialog visibility
const faceToLabel = ref<DetectedFace | null>(null); // Add for face labeling

const internalMemory = computed<MemoryDto>({
  get: () => props.modelValue,
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

watch(() => memoryFaceStore.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

const handleFileUpload = async (file: File | File[] | null) => {
  if (props.readonly) return;

  if (file instanceof File) {
    await memoryFaceStore.detectFaces(file);
    if (memoryFaceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id';
      internalMemory.value.faces = memoryFaceStore.detectedFaces;
      internalMemory.value.photoUrl = memoryFaceStore.uploadedImage;
    } else if (!memoryFaceStore.loading && memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  } else if (Array.isArray(file) && file.length > 0) {
    await memoryFaceStore.detectFaces(file[0]);
    if (memoryFaceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id';
      internalMemory.value.faces = memoryFaceStore.detectedFaces;
      internalMemory.value.photoUrl = memoryFaceStore.uploadedImage;
    } else if (!memoryFaceStore.loading && memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  }
  else {
    memoryFaceStore.resetState();
    internalMemory.value.photoAnalysisId = undefined;
    internalMemory.value.faces = [];
    internalMemory.value.photoUrl = undefined;
  }
};

const openSelectMemberDialog = (faceId: string) => {
  memoryFaceStore.selectFace(faceId); // Select face in store
  const face = memoryFaceStore.currentSelectedFace;
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const handleLabelFaceAndCloseDialog = (faceId: string, memberDetails: Member) => {
  memoryFaceStore.labelFace(faceId, memberDetails.id, memberDetails);
  showSelectMemberDialog.value = false;
  faceToLabel.value = null;
};

const handleRemoveFace = (faceId: string) => {
  memoryFaceStore.removeFace(faceId);
};

defineExpose({
  isValid: computed(() => !memoryFaceStore.loading),
  memoryFaceStore,
  faceUploadInputRef,
});
</script>