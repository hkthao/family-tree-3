<template>
  <v-card :elevation="0" data-testid="member-face-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isDetectingFaces || isSaving" indeterminate color="primary"
      class="my-4"></v-progress-linear>
    <v-card-text>
      <FamilyAutocomplete v-model="selectedFamilyId" :label="t('memberFace.form.family')" class="mb-4"
        :disabled="!!props.familyId" />
      <div v-if="selectedFamilyId">
        <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" />

        <div v-if="uploadedImage && detectedFaces.length > 0" class="mt-4">
          <FaceBoundingBoxViewer :image-src="uploadedImage" :faces="detectedFaces" selectable
            @face-selected="openSelectMemberDialog" class="mb-4" /> <!-- Added mb-4 for spacing -->
          <FaceDetectionSidebar :faces="detectedFaces" @face-selected="openSelectMemberDialog"
            @remove-face="handleRemoveFace" />
        </div>
        <v-alert v-else-if="!isDetectingFaces && !uploadedImage" type="info" class="my-4">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!isDetectingFaces && uploadedImage && detectedFaces.length === 0" type="info"
          class="my-4">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>

      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" :disabled="!canSaveLabels" @click="saveAllLabeledFaces" :loading="isSaving">
        {{ t('face.recognition.saveAllLabeledFacesButton') }}
      </v-btn>
    </v-card-actions>
    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="selectedFamilyId" />
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useMemberFaceAdd } from '@/composables';

interface MemberFaceAddViewProps {
  memberId?: string; // Optional, might be pre-selected if adding from a member detail page
  familyId?: string; // Optional, might be pre-selected if adding from a family context
}

const props = defineProps<MemberFaceAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();

const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);

const {
  state: {
    isDetectingFaces,
    isSaving,
    showSelectMemberDialog,
    faceToLabel,
    selectedFamilyId,
    uploadedImage,
    detectedFaces,
    canSaveLabels,
  },
  actions: {
    handleFileUpload: composableHandleFileUpload,
    openSelectMemberDialog,
    handleLabelFaceAndCloseDialog,
    handleRemoveFace,
    saveAllLabeledFaces,
    closeForm: composableCloseForm,
    resetState: composableResetState,
  },
} = useMemberFaceAdd({
  memberId: props.memberId,
  familyId: props.familyId,
  onSaved: () => emit('saved'),
  onClosed: () => emit('close'),
  t, // Pass t function to the composable
});

// Wrapper for handleFileUpload to include resetting the FaceUploadInput component
const handleFileUpload = async (file: File | File[] | null) => {
  if (!file) {
    composableResetState(); // Reset composable state
    if (faceUploadInputRef.value) {
      faceUploadInputRef.value.reset(); // Reset the component itself
    }
    return;
  }
  await composableHandleFileUpload(file);
};

// Wrapper for closeForm to also reset FaceUploadInput
const closeForm = () => {
  composableCloseForm(); // Call composable's close form logic
  if (faceUploadInputRef.value) {
    faceUploadInputRef.value.reset();
  }
};

defineExpose({
  handleFileUpload,
  saveAllLabeledFaces,
  canSaveLabels,
  closeForm, // Expose the wrapped closeForm
});</script>
