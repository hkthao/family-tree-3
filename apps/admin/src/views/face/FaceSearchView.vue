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
        <v-progress-linear v-if="isDetectingFaces" indeterminate color="primary" class="my-4"></v-progress-linear>
        <div v-if="uploadedImage && detectedFaces.length > 0" class="mt-4">
          <v-row>
            <v-col cols="12" md="8">
              <FaceBoundingBoxViewer id="tour-face-viewer" :image-src="uploadedImage"
                :faces="detectedFaces" selectable />
            </v-col>
            <v-col cols="12" md="4">
              <FaceDetectionSidebar id="tour-face-sidebar" :readOnly="true" :faces="detectedFaces" />
            </v-col>
          </v-row>
        </div>
        <v-alert v-else-if="!isDetectingFaces && !uploadedImage" type="info" class="my-4">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!isDetectingFaces && uploadedImage && detectedFaces.length === 0"
          type="info" class="my-4">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>
      </div>
      <v-alert v-else type="info" class="my-4">{{ t('face.selectFamilyToUpload') }}</v-alert>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';
import { useFaceSearchTour } from '@/composables';
import { useGlobalSnackbar } from '@/composables';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useDetectFacesMutation } from '@/composables/member-face';
import type { DetectedFace } from '@/types';

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

useFaceSearchTour();

const { mutate: detectFaces, isPending: isDetectingFaces, error: detectError } = useDetectFacesMutation();

const selectedFamilyId = ref<string | undefined>(undefined);
const uploadedImage = ref<string | null | undefined>(undefined);
const detectedFaces = ref<DetectedFace[]>([]);
const originalImageUrl = ref<string | null>(null);

watch(detectError, (newError) => {
  if (newError) {
    showSnackbar(newError.message, 'error');
  }
});

const resetState = () => {
  uploadedImage.value = null;
  detectedFaces.value = [];
  originalImageUrl.value = null;
};

const handleFileUpload = async (file: File | null) => {
  resetState();
  if (!file) {
    return;
  }
  if (!selectedFamilyId.value) {
    showSnackbar(t('face.selectFamilyToUpload'), 'warning');
    return;
  }
  detectFaces({ imageFile: file, familyId: selectedFamilyId.value, resize: true }, {
    onSuccess: (data) => {
      uploadedImage.value = data.originalImageBase64;
      detectedFaces.value = data.detectedFaces;
      originalImageUrl.value = data.originalImageUrl;
    },
    onError: (error) => {
      showSnackbar(error.message, 'error');
    },
  });
};
</script>
