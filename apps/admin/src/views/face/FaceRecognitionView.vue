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
        type="info" class="my-4">{{ t('face.recognition.noFacesDetected') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="primary" :disabled="!canSaveLabels" @click="saveLabels">
        {{ t('face.recognition.saveLabelsButton') }}
      </v-btn>
    </v-card-actions>
    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="props.familyId" />
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFaceStore } from '@/stores/face.store';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import type { DetectedFace, Member } from '@/types';
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

const openSelectMemberDialog = (faceId: string) => {
  const face = faceStore.detectedFaces.find(f => f.id === faceId);
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const canSaveLabels = computed(() => {
  return faceStore.detectedFaces.some(
    (face) =>
      face.memberId && // Must have a memberId assigned
      (face.originalMemberId === null || // Was unlabeled, now labeled
        face.originalMemberId === undefined || // Was unlabeled, now labeled
        face.memberId !== face.originalMemberId), // Label has changed
  );
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
  const result = await faceStore.saveFaceLabels();
  if (result.ok) {
    showSnackbar(t('face.recognition.saveSuccess'), 'success');
    faceStore.resetState(); // Reset face store after saving
    if (faceUploadInputRef.value) {
      faceUploadInputRef.value.reset(); // Clear the file input
    }
  }
};
</script>