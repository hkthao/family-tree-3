<template>
  <v-container fluid>
    <v-card class="pa-4" elevation="2">
      <v-card-title class="text-h5">{{ t('face.labeling.title') }}</v-card-title>
      <v-card-text>
        <v-alert v-if="faceStore.error" type="error" class="my-4">{{ faceStore.error }}</v-alert>

        <v-row v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0">
          <v-col cols="12" md="8">
            <FaceBoundingBoxViewer
              :image-src="faceStore.uploadedImage"
              :faces="faceStore.detectedFaces"
              @face-selected="faceStore.selectFace"
              :selected-face-id="faceStore.selectedFaceId"
            />
          </v-col>
          <v-col cols="12" md="4">
            <FaceDetectionSidebar
              :faces="faceStore.detectedFaces"
              :selected-face-id="faceStore.selectedFaceId"
              @face-selected="faceStore.selectFace"
            />
            <v-divider class="my-4"></v-divider>
            <FaceLabelCard
              v-if="faceStore.currentSelectedFace"
              :face="faceStore.currentSelectedFace"
              @save-mapping="handleSaveMapping"
            />
            <v-alert v-else type="info" class="mt-4">{{ t('face.labeling.selectFacePrompt') }}</v-alert>
          </v-col>
        </v-row>
        <v-alert v-else type="warning" class="my-4">{{ t('face.labeling.noFacesDetected') }}</v-alert>
      </v-card-text>
      <v-card-actions class="justify-end">
        <v-btn color="secondary" @click="router.push({ name: 'FaceUpload' })">
          {{ t('common.back') }}
        </v-btn>
        <v-btn color="primary" @click="handleFinishLabeling">
          {{ t('face.labeling.finishButton') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFaceStore } from '@/stores/face.store';
import { FaceBoundingBoxViewer, FaceDetectionSidebar, FaceLabelCard } from '@/components/face';

const { t } = useI18n();
const router = useRouter();
const faceStore = useFaceStore();

const handleSaveMapping = async (faceId: string, memberId: string) => {
  await faceStore.saveFaceMapping(faceId, memberId);
};

const handleFinishLabeling = () => {
  // Optionally, navigate to a summary page or back to FaceUploadView
  faceStore.resetState(); // Clear state after finishing
  router.push({ name: 'FaceUpload' });
};
</script>
