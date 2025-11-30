<template>
  <v-card :elevation="0">
    <v-card-text>
      <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" />
      <v-progress-linear v-if="faceStore.loading" indeterminate color="primary" class="my-4"></v-progress-linear>
      <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
        <v-row>
          <v-col cols="12" md="8">
            <FaceBoundingBoxViewer :image-src="faceStore.uploadedImage" :faces="faceStore.detectedFaces" selectable
              @face-selected="openSelectMemberDialog" />
          </v-col>
          <v-col cols="12" md="4">
            <FaceDetectionSidebar :faces="faceStore.detectedFaces" @face-selected="openSelectMemberDialog"
              @remove-face="handleRemoveFace" />
          </v-col>
        </v-row>
      </div>
      <v-alert v-else-if="!faceStore.loading && !faceStore.uploadedImage" type="info" class="my-4">{{
        t('face.recognition.uploadPrompt') }}</v-alert>
      <v-alert v-else-if="!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0"
        type="info" class="my-4">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>
    </v-card-text>
    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="props.familyId" />
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFaceStore } from '@/stores/face.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import type { DetectedFace } from '@/types';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

interface FaceRecognitionViewProps {
  familyId?: string;
}

const props = defineProps<FaceRecognitionViewProps>();

const { t } = useI18n();
const faceStore = useFaceStore();
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar
const showSelectMemberDialog = ref(false);
const faceToLabel = ref<DetectedFace | null>(null);
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null); // Ref for FaceUploadInput

watch(() => faceStore.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

const handleFileUpload = async (file: File | File[] | null) => {
  if (file instanceof File) {
    await faceStore.detectFaces(file, false);
  } else if (Array.isArray(file) && file.length > 0) {
    // Handle multiple files if needed, but for now, we expect a single file
    await faceStore.detectFaces(file[0], false);
  } else {
    // Clear detected faces if no file or null is uploaded
    faceStore.resetState();
  }
};

const openSelectMemberDialog = (face: DetectedFace) => {
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const handleLabelFaceAndCloseDialog = (updatedFace: DetectedFace) => {
  const index = faceStore.detectedFaces.findIndex(f => f.id === updatedFace.id);
  if (index !== -1) {
    faceStore.detectedFaces.splice(index, 1, updatedFace); // Replace the old face with the updated one
  }
  showSelectMemberDialog.value = false;
  faceToLabel.value = null;
};

const handleRemoveFace = (faceId: string) => {
  faceStore.removeFace(faceId);
};
</script>
