<template>
  <v-container fluid>
    <v-card class="pa-4" elevation="2">
      <v-card-title class="text-h5">{{ t('face.upload.title') }}</v-card-title>
      <v-card-text>
        <FaceUploadInput @file-uploaded="handleFileUpload" />

        <v-progress-linear
          v-if="faceStore.loading"
          indeterminate
          color="primary"
          class="my-4"
        ></v-progress-linear>

        <v-alert v-if="faceStore.error" type="error" class="my-4">{{ faceStore.error }}</v-alert>

        <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
          <v-row>
            <v-col cols="12" md="8">
              <FaceBoundingBoxViewer
                :image-src="faceStore.uploadedImage"
                :faces="faceStore.detectedFaces"
                selectable
                @face-selected="faceStore.selectFace"
              />
            </v-col>
            <v-col cols="12" md="4">
              <FaceDetectionSidebar
                :faces="faceStore.detectedFaces"
                :selected-face-id="faceStore.selectedFaceId"
                @face-selected="faceStore.selectFace"
              />
            </v-col>
          </v-row>
        </div>
      </v-card-text>
      <v-card-actions class="justify-end">
        <v-btn
          color="primary"
          :disabled="!faceStore.uploadedImage || faceStore.detectedFaces.length === 0"
          @click="goToLabeling"
        >
          {{ t('face.upload.continueButton') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFaceStore } from '@/stores/face.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';

const { t } = useI18n();
const router = useRouter();
const faceStore = useFaceStore();

const handleFileUpload = async (file: File) => {
  await faceStore.detectFaces(file);
};

const goToLabeling = () => {
  router.push({ name: 'FaceLabeling' });
};
</script>
