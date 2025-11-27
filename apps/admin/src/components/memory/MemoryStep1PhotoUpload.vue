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

          <!-- Target Member Selection -->
          <v-card flat>
            <v-card-title>{{ t('memory.create.selectTargetMember') }}</v-card-title>
            <v-card-text>
              <v-chip-group v-model="selectedTargetMemberFaceId" mandatory column>
                <v-chip v-for="face in memoryStore.faceRecognition.detectedFaces" :key="face.id" :value="face.id"
                  variant="outlined" color="primary" filter>
                  <v-avatar left>
                    <v-img :src="createBase64ImageSrc(face.thumbnail)" alt="Face"></v-img>
                  </v-avatar>
                  <span class="ml-2">{{ face.memberName || t('common.unknown') }}</span>
                  <v-chip v-if="face.emotion"  class="ml-2">{{ face.emotion }}</v-chip>
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
import type { MemoryDto, ExifDataDto } from '@/types/memory'; // Added ExifDataDto
import type { DetectedFace, Member } from '@/types';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { createBase64ImageSrc } from '@/utils/image.utils'; // NEW IMPORT

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
const selectedTargetMemberFaceId = ref<string | null>(null); // Renamed ref

const internalMemory = computed<MemoryDto>({
  get: () => props.modelValue,
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

watch(() => memoryStore.faceRecognition.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

// Watch for changes in selectedTargetMemberFaceId to update internalMemory.memberId and targetFaceId
watch(selectedTargetMemberFaceId, (newId) => { // Updated ref name
  internalMemory.value.targetFaceId = newId ?? undefined; // Set targetFaceId, convert null to undefined
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

// Helper function to load image and get dimensions
const loadImage = (file: File): Promise<HTMLImageElement> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = reject;
    img.src = URL.createObjectURL(file);
  });
};

// Function to simulate EXIF extraction (placeholder)
const extractExifData = async (file: File): Promise<ExifDataDto | undefined> => {
  // In a real application, you would use a library like 'piexifjs' or similar
  // to parse the EXIF data from the image file.
  // For this task, we'll return some dummy data or undefined.
  console.log('Attempting to extract EXIF from file:', file.name);
  // Example dummy data
  return {
    datetime: new Date().toISOString(),
    gps: '50.123, 10.456', // Example coordinates
    cameraInfo: 'DummyCameraModel',
  };
  // If no EXIF data is found or library is not available:
  // return undefined;
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
    await memoryStore.detectFaces(uploadedFile, true);

    // Set image size
    try {
      const img = await loadImage(uploadedFile);
      internalMemory.value.imageSize = `${img.width}x${img.height}`;
    } catch (e) {
      console.error('Failed to load image for dimensions:', e);
      internalMemory.value.imageSize = undefined; // Clear or set default
    }

    // Extract EXIF data
    try {
      internalMemory.value.exifData = await extractExifData(uploadedFile);
    } catch (e) {
      console.error('Failed to extract EXIF data:', e);
      internalMemory.value.exifData = undefined;
    }

    if (memoryStore.faceRecognition.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id';
      internalMemory.value.faces = memoryStore.faceRecognition.detectedFaces;
      internalMemory.value.photoUrl = memoryStore.faceRecognition.uploadedImage;
      internalMemory.value.photo = memoryStore.faceRecognition.uploadedImage; // Assign to the new 'photo' property
      // Automatically select the first face as main character if any faces are detected
      if (memoryStore.faceRecognition.detectedFaces.length > 0) {
        selectedTargetMemberFaceId.value = memoryStore.faceRecognition.detectedFaces[0].id; // Updated ref name
      }
    } else if (!memoryStore.faceRecognition.loading && memoryStore.faceRecognition.uploadedImage && memoryStore.faceRecognition.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
      internalMemory.value.faces = []; // Clear faces if none detected
      selectedTargetMemberFaceId.value = null; // Updated ref name
      internalMemory.value.photo = undefined; // Clear photo if no faces detected
      internalMemory.value.imageSize = undefined;
      internalMemory.value.exifData = undefined;
    }
  } else {
    memoryStore.resetFaceRecognitionState();
    internalMemory.value.photoAnalysisId = undefined;
    internalMemory.value.faces = [];
    internalMemory.value.photoUrl = undefined;
    internalMemory.value.photo = undefined; // Clear the 'photo' property
    selectedTargetMemberFaceId.value = null; // Updated ref name
    internalMemory.value.imageSize = undefined;
    internalMemory.value.exifData = undefined;
  }
};

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
  if (selectedTargetMemberFaceId.value === faceId) { // Updated ref name
    internalMemory.value.memberId = memberDetails.id;
  }
};

const handleRemoveFace = (faceId: string) => {
  memoryStore.removeFace(faceId);
  // Synchronize internalMemory.value.faces with the updated detectedFaces from the store
  internalMemory.value.faces = memoryStore.faceRecognition.detectedFaces;
  // If the removed face was the main character, clear selection
  if (selectedTargetMemberFaceId.value === faceId) { // Updated ref name
    selectedTargetMemberFaceId.value = null;
  }
};

defineExpose({
  isValid: computed(() => !memoryStore.faceRecognition.loading),
  memoryFaceStore: memoryStore.faceRecognition, // Expose memoryStore.faceRecognition instead of memoryFaceStore
  faceUploadInputRef,
  selectedTargetMemberFaceId, // Updated exposed ref name
});
</script>

<style scoped>
</style>