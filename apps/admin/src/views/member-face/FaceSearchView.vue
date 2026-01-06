<template>
  <v-card elevation="2">
    <v-card-text>
      <div v-if="hasConsent">
        <FaceUploadInput id="tour-face-upload" @file-uploaded="handleFileUploadWrapper" />
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
        <v-alert v-else-if="!isDetectingFaces && !uploadedImage" type="info" class="my-4">{{ t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!isDetectingFaces && uploadedImage && detectedFaces.length === 0" type="info" class="my-4">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>
      </div>
      <div v-else class="text-center pa-5">
        <FaceDataConsentCard
          :handle-grant-consent="handleGrantConsent"
          :handle-deny-consent="handleDenyConsent"
        />
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face';
import { useFaceSearchTour } from '@/composables';
import { useFaceSearch } from '@/composables';
import { onMounted, ref } from 'vue'; // Added ref
import { useI18n } from 'vue-i18n';
import { useFaceDataConsent } from '@/composables/useFaceDataConsent';
import { useGlobalSnackbar } from '@/composables';
import FaceDataConsentCard from '@/components/common/FaceDataConsentCard.vue';

const props = defineProps<{
  familyId?: string;
}>();

const { t } = useI18n();
const { checkConsent, setConsent, denyConsent } = useFaceDataConsent(); // Removed faceConsentDialogRef
const { showSnackbar } = useGlobalSnackbar();

const hasConsent = ref(false); // NEW

const {
  state: { selectedFamilyId, uploadedImage, detectedFaces, isDetectingFaces },
  actions: { handleFileUpload },
} = useFaceSearch();

// Wrapper for handleFileUpload to include consent check
const handleFileUploadWrapper = async (file: File | File[] | null) => {
  if (!file) {
    return;
  }

  if (!hasConsent.value) { // Check hasConsent.value directly
    showSnackbar(t('faceDataConsent.deniedMessage'), 'error');
    return;
  }

  // Ensure only a single File object is passed, as useFaceSearch expects File | null
  const fileToProcess = Array.isArray(file) ? file[0] : file;
  if (!fileToProcess) {
    return; // Should not happen if file is not null/empty array
  }

  handleFileUpload(fileToProcess);
};

const handleGrantConsent = () => {
  setConsent();
  hasConsent.value = true;
  showSnackbar(t('faceDataConsent.grantSuccess'), 'success');
};

const handleDenyConsent = () => {
  denyConsent();
  hasConsent.value = false;
  showSnackbar(t('faceDataConsent.denySuccess'), 'info');
};

onMounted(() => {
  if (props.familyId) {
    selectedFamilyId.value = props.familyId;
  }
  hasConsent.value = checkConsent(); // Check consent on mount
});

useFaceSearchTour();
</script>