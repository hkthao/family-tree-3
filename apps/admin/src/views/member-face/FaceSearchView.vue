<template>
  <v-card elevation="2">
    <v-card-text>
      <div>
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
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';
import { useFaceSearchTour } from '@/composables';
import { useFaceSearch } from '@/composables';
import { onMounted } from 'vue';

const props = defineProps<{
  familyId?: string;
}>();

const {
  state: { selectedFamilyId, uploadedImage, detectedFaces, isDetectingFaces, t },
  actions: { handleFileUpload },
} = useFaceSearch();

onMounted(() => {
  if (props.familyId) {
    selectedFamilyId.value = props.familyId;
  }
});

useFaceSearchTour(); // Still use the tour composable directly here
</script>
