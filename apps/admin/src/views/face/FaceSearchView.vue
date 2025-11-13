<template>
  <v-card elevation="2">
    <v-card-text>
      <FaceUploadInput id="tour-face-upload" @file-uploaded="handleFileUpload" />
      <v-progress-linear v-if="faceStore.loading" indeterminate color="primary" class="my-4"></v-progress-linear>
      <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
        <v-row>
          <v-col cols="12" md="8">
            <FaceBoundingBoxViewer id="tour-face-viewer" :image-src="faceStore.uploadedImage" :faces="faceStore.detectedFaces" selectable />
          </v-col>
          <v-col cols="12" md="4">
            <FaceDetectionSidebar id="tour-face-sidebar" :readOnly="true" :faces="faceStore.detectedFaces" />
          </v-col>
        </v-row>
      </div>
      <v-alert v-else-if="!faceStore.loading && !faceStore.uploadedImage" type="info" class="my-4">{{
        t('face.recognition.uploadPrompt') }}</v-alert>
      <v-alert v-else-if="!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0"
        type="info" class="my-4">{{ t('face.recognition.noFacesDetected') }}</v-alert>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFaceStore } from '@/stores/face.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';
import { useFaceSearchTour } from '@/composables';

const { t } = useI18n();
const faceStore = useFaceStore();
const notificationStore = useNotificationStore();

useFaceSearchTour();

watch(() => faceStore.error, (newError) => {
  if (newError) {
    notificationStore.showSnackbar(newError, 'error');
  }
});

const handleFileUpload = async (file: File | null) => {
  if (!file) {
    faceStore.resetState();
    return;
  }
  await faceStore.detectFaces(file);
};

</script>
