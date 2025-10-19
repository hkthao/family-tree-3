<template>
  <v-card class="pa-4" elevation="2">
    <v-card-title class="text-h5">{{ t('face.recognition.title') }}</v-card-title>
    <v-card-text>
      <FaceUploadInput @file-uploaded="handleFileUpload" />

      <v-progress-linear v-if="faceStore.loading" indeterminate color="primary" class="my-4"></v-progress-linear>

      <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
        <v-row>
          <v-col cols="12" md="8">
            <FaceBoundingBoxViewer :image-src="faceStore.uploadedImage" :faces="faceStore.detectedFaces" selectable
              @face-selected="openSelectMemberDialog" />
          </v-col>
          <v-col cols="12" md="4">
            <FaceDetectionSidebar :faces="faceStore.detectedFaces" @face-selected="openSelectMemberDialog" @remove-face="handleRemoveFace" />
          </v-col>
        </v-row>
      </div>
      <v-alert v-else-if="!faceStore.loading && !faceStore.uploadedImage" type="info" class="my-4">{{
        t('face.recognition.uploadPrompt') }}</v-alert>
      <v-alert v-else-if="!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0"
        type="info" class="my-4">{{ t('face.recognition.noFacesDetected') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="primary" :disabled="!canSaveLabels" @click="saveLabels">
        {{ t('face.recognition.saveLabelsButton') }}
      </v-btn>
    </v-card-actions>

    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" />
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFaceStore } from '@/stores/face.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import type { DetectedFace, Member } from '@/types';

const { t } = useI18n();

const faceStore = useFaceStore();
const notificationStore = useNotificationStore();

const showSelectMemberDialog = ref(false);
const faceToLabel = ref<DetectedFace | null>(null);

watch(() => faceStore.error, (newError) => {
  if (newError) {
    notificationStore.showSnackbar(newError, 'error');
  }
});

const handleFileUpload = async (file: File) => {
  await faceStore.detectFaces(file);
};

const openSelectMemberDialog = (faceId: string) => {
  const face = faceStore.detectedFaces.find(f => f.id === faceId);
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const canSaveLabels = computed(() => {
  return faceStore.detectedFaces.length > 0 && faceStore.detectedFaces.every(face => face.memberId !== null && face.memberId !== undefined);
});

const handleLabelFaceAndCloseDialog = (faceId: string, memberDetails: Member) => {
  faceStore.labelFace(faceId, memberDetails.id, memberDetails);
  showSelectMemberDialog.value = false;
  faceToLabel.value = null;
};

const handleRemoveFace = (faceId: string) => {
  faceStore.removeFace(faceId);
};

const saveLabels = async () => {
  const success = await faceStore.saveFaceLabels();
  if (success) {
    notificationStore.showSnackbar(t('face.recognition.saveSuccess'), 'success');
    faceStore.resetState(); // Reset face store after saving
  }
};
</script>

<style scoped>
/* Add any specific styles for this view here */
</style>
