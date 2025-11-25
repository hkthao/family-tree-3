<template>
  <div>
    <v-row>
      <v-col v-if="memoryFaceStore.loading" cols="12">
        <v-progress-linear v-if="memoryFaceStore.loading" indeterminate color="primary"></v-progress-linear>
      </v-col>
      <v-col cols="12">
        <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" :readonly="readonly" />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <div v-if="memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length > 0">
          <FaceBoundingBoxViewer :image-src="memoryFaceStore.uploadedImage" :faces="memoryFaceStore.detectedFaces" />
          <FaceDetectionSidebar :faces="memoryFaceStore.detectedFaces" />
        </div>
        <v-alert v-else-if="!memoryFaceStore.loading && !memoryFaceStore.uploadedImage" type="info">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!memoryFaceStore.loading && memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length === 0"
          type="info">{{ t('face.recognition.noFacesDetected') }}</v-alert>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryFaceStore } from '@/stores/memoryFaceStore'; // Use the new store
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';
import type { MemoryDto } from '@/types/memory';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const memoryFaceStore = useMemoryFaceStore(); // Initialize the new store
const { showSnackbar } = useGlobalSnackbar();
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);

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
  if (props.readonly) return; // Do nothing if readonly

  if (file instanceof File) {
    await memoryFaceStore.detectFaces(file);
    if (memoryFaceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id'; // Placeholder for analysis ID
      internalMemory.value.faces = memoryFaceStore.detectedFaces;
      internalMemory.value.photoUrl = memoryFaceStore.uploadedImage;
    } else if (!memoryFaceStore.loading && memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  } else if (Array.isArray(file) && file.length > 0) {
    await memoryFaceStore.detectFaces(file[0]);
    if (memoryFaceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id'; // Placeholder for analysis ID
      internalMemory.value.faces = memoryFaceStore.detectedFaces;
      internalMemory.value.photoUrl = memoryFaceStore.uploadedImage;
    } else if (!memoryFaceStore.loading && memoryFaceStore.uploadedImage && memoryFaceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  } else {
    memoryFaceStore.resetState();
    internalMemory.value.photoAnalysisId = undefined;
    internalMemory.value.faces = [];
    internalMemory.value.photoUrl = undefined;
  }
};

// No explicit form validation for this step, but we can expose state
defineExpose({
  isValid: computed(() => !memoryFaceStore.loading), // Step is valid if not loading
  memoryFaceStore,
  faceUploadInputRef,
});
</script>