<template>
  <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" :readonly="readonly" />
  <v-progress-linear v-if="faceStore.loading" indeterminate color="primary" class="my-4"></v-progress-linear>
  <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
    <v-row>
      <v-col cols="12" md="8">
        <FaceBoundingBoxViewer :image-src="faceStore.uploadedImage" :faces="faceStore.detectedFaces" />
      </v-col>
      <v-col cols="12" md="4">
        <FaceDetectionSidebar :faces="faceStore.detectedFaces" />
      </v-col>
    </v-row>
  </div>
  <v-alert v-else-if="!faceStore.loading && !faceStore.uploadedImage" type="info" class="my-4">{{
    t('face.recognition.uploadPrompt') }}</v-alert>
  <v-alert v-else-if="!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0"
    type="info" class="my-4">{{ t('face.recognition.noFacesDetected') }}</v-alert>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFaceStore } from '@/stores/face.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';
import type { MemoryDto } from '@/types/memory';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const faceStore = useFaceStore();
const { showSnackbar } = useGlobalSnackbar();
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);

const internalMemory = computed<MemoryDto>({
  get: () => props.modelValue,
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

watch(() => faceStore.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

const handleFileUpload = async (file: File | File[] | null) => {
  if (props.readonly) return; // Do nothing if readonly

  if (file instanceof File) {
    await faceStore.detectFaces(file);
    if (faceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id'; // Placeholder for analysis ID
      internalMemory.value.faces = faceStore.detectedFaces;
      internalMemory.value.photoUrl = faceStore.uploadedImage;
    } else if (!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  } else if (Array.isArray(file) && file.length > 0) {
    await faceStore.detectFaces(file[0]);
    if (faceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id'; // Placeholder for analysis ID
      internalMemory.value.faces = faceStore.detectedFaces;
      internalMemory.value.photoUrl = faceStore.uploadedImage;
    } else if (!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  } else {
    faceStore.resetState();
    internalMemory.value.photoAnalysisId = undefined;
    internalMemory.value.faces = [];
    internalMemory.value.photoUrl = undefined;
  }
};

// No explicit form validation for this step, but we can expose state
defineExpose({
  isValid: computed(() => !faceStore.loading), // Step is valid if not loading
  faceStore,
  faceUploadInputRef,
});
</script>
