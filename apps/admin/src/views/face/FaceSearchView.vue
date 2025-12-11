<template>
  <v-card elevation="2">
    <v-card-text>
      <v-row>
        <v-col cols="4">
          <FamilyAutocomplete
            v-model="selectedFamilyId"
            :label="t('face.family')"
            class="mb-4"
            data-testid="family-autocomplete"
          />
        </v-col>
      </v-row>
      <div v-if="selectedFamilyId">
        <FaceUploadInput id="tour-face-upload" @file-uploaded="handleFileUpload" />
        <v-progress-linear v-if="faceStore.loading" indeterminate color="primary" class="my-4"></v-progress-linear>
        <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
          <v-row>
            <v-col cols="12" md="8">
              <FaceBoundingBoxViewer id="tour-face-viewer" :image-src="faceStore.uploadedImage"
                :faces="faceStore.detectedFaces" selectable />
            </v-col>
            <v-col cols="12" md="4">
              <FaceDetectionSidebar id="tour-face-sidebar" :readOnly="true" :faces="faceStore.detectedFaces" />
            </v-col>
          </v-row>
        </div>
        <v-alert v-else-if="!faceStore.loading && !faceStore.uploadedImage" type="info" class="my-4">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0"
          type="info" class="my-4">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>
      </div>
      <v-alert v-else type="info" class="my-4">{{ t('face.selectFamilyToUpload') }}</v-alert>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFaceStore } from '@/stores/face.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';
import { useFaceSearchTour } from '@/composables';
import { useGlobalSnackbar } from '@/composables';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';

const { t } = useI18n();
const faceStore = useFaceStore();
const { showSnackbar } = useGlobalSnackbar();

useFaceSearchTour();

const selectedFamilyId = ref<string | undefined>(undefined);

watch(() => faceStore.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

const handleFileUpload = async (file: File | null) => {
  if (!file) {
    faceStore.resetState();
    return;
  }
  if (!selectedFamilyId.value) {
    showSnackbar(t('face.selectFamilyToUpload'), 'warning');
    return;
  }
  await faceStore.detectFaces(file, selectedFamilyId.value, true); // Assuming 'true' means search for similar faces
};
</script>
