<template>
  <v-card :elevation="0" data-testid="member-face-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="faceStore.loading || memberFaceStore.add.loading" indeterminate color="primary"
      class="my-4"></v-progress-linear>
    <v-card-text>
      <FamilyAutocomplete v-model="selectedFamilyId" :label="t('memberFace.form.family')" class="mb-4"
        :disabled="!!props.familyId" />
      <div v-if="selectedFamilyId">
        <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" />

        <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
          <FaceBoundingBoxViewer :image-src="faceStore.uploadedImage" :faces="faceStore.detectedFaces" selectable
            @face-selected="openSelectMemberDialog" class="mb-4" /> <!-- Added mb-4 for spacing -->
          <FaceDetectionSidebar :faces="faceStore.detectedFaces" @face-selected="openSelectMemberDialog"
            @remove-face="handleRemoveFace" />
        </div>
        <v-alert v-else-if="!faceStore.loading && !faceStore.uploadedImage" type="info" class="my-4">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0"
          type="info" class="my-4">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>

      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" :disabled="!canSaveLabels" @click="saveAllLabeledFaces"
        :loading="memberFaceStore.add.loading">
        {{ t('face.recognition.saveAllLabeledFacesButton') }}
      </v-btn>
    </v-card-actions>
    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="selectedFamilyId" />
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberFaceStore } from '@/stores/member-face.store';
import { useFaceStore } from '@/stores/face.store'; // NEW
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face'; // NEW
import type { DetectedFace, MemberFace } from '@/types'; // NEW
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue'; // NEW

interface MemberFaceAddViewProps {
  memberId?: string; // Optional, might be pre-selected if adding from a member detail page
  familyId?: string; // Optional, might be pre-selected if adding from a family context
}

const props = defineProps<MemberFaceAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memberFaceStore = useMemberFaceStore();
const faceStore = useFaceStore(); // NEW
const { showSnackbar } = useGlobalSnackbar();

const showSelectMemberDialog = ref(false);
const faceToLabel = ref<DetectedFace | null>(null);
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);
const selectedFamilyId = ref<string | undefined>(props.familyId); // NEW

watch(() => props.familyId, (newFamilyId) => {
  selectedFamilyId.value = newFamilyId;
});

// Watch for faceStore errors
watch(() => faceStore.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

const handleFileUpload = async (file: File | File[] | null) => {
  if (file instanceof File) {
    await faceStore.detectFaces(file, selectedFamilyId.value!, false); // Assuming false means no resize for analysis here
  } else if (Array.isArray(file) && file.length > 0) {
    await faceStore.detectFaces(file[0], selectedFamilyId.value!, false);
  } else {
    faceStore.resetState();
  }
};

const openSelectMemberDialog = (face: DetectedFace) => {
  if (face.status === 'recognized') { // NEW: Add this check
    return;
  }
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const handleLabelFaceAndCloseDialog = (updatedFace: DetectedFace) => {
  const index = faceStore.detectedFaces.findIndex(f => f.id === updatedFace.id);
  if (index !== -1) {
    faceStore.detectedFaces.splice(index, 1, updatedFace); // Replace the old face with the updated one
  }
  showSelectMemberDialog.value = false;
  faceToLabel.value = null;
};

const handleRemoveFace = (faceId: string) => {
  faceStore.removeFace(faceId);
};

const canSaveLabels = computed(() => {
  if (!selectedFamilyId.value) { // NEW: Cannot save if no family is selected
    return false;
  }
  const hasFacesToSave = faceStore.detectedFaces.some(face =>
    (face.status === 'labeled') || // User explicitly labeled this face
    (face.status === 'unrecognized' && face.memberId !== null) || // User labeled a previously unrecognized face
    (face.status === 'recognized' && face.originalMemberId !== face.memberId) // User changed a recognized face's label
  );

  const hasUnlabeledFacesLeft = faceStore.detectedFaces.some(face =>
    face.status === 'unrecognized' && face.memberId === null // Still an unrecognized face that hasn't been labeled
  );

  return hasFacesToSave && !hasUnlabeledFacesLeft;
});

const saveAllLabeledFaces = async () => {
  if (!selectedFamilyId.value) {
    showSnackbar(t('memberFace.messages.noFamilySelected'), 'warning');
    return;
  }
  // Filter out unlabeled faces and transform DetectedFace to MemberFace for creation
  const facesToSave = faceStore.detectedFaces
    .filter(face => face.memberId)
    .map(face => {
      // Create a new MemberFace object from DetectedFace properties
      const memberFace: Omit<MemberFace, 'id'> = {
        memberId: face.memberId!,
        faceId: face.id, // Use the detected face ID as the backend FaceId
        boundingBox: face.boundingBox,
        confidence: face.confidence,
        thumbnail: face.thumbnail, // NEW: Include thumbnail (base64)
        thumbnailUrl: face.thumbnailUrl, // Use the detected thumbnail URL
        originalImageUrl: faceStore.originalImageUrl, // The original uploaded image URL
        embedding: face.embedding || [],
        emotion: face.emotion,
        emotionConfidence: face.emotionConfidence,
        isVectorDbSynced: false, // Will be updated by backend after n8n sync
        vectorDbId: undefined, // Will be set by backend
        familyId: selectedFamilyId.value!, // NEW: Add familyId from selectedFamilyId
      };
      return memberFace;
    });

  if (facesToSave.length === 0) {
    showSnackbar(t('memberFace.messages.noFacesToSave'), 'warning');
    return;
  }

  // Iterate and add each member face
  let allSucceeded = true;
  for (const memberFace of facesToSave) {
    // Override memberId if prop is provided, to ensure consistency
    if (props.memberId) {
      memberFace.memberId = props.memberId;
    }
    const result = await memberFaceStore.addItem(memberFace);
    if (!result.ok) {
      allSucceeded = false;
      showSnackbar(t('memberFace.messages.saveErrorForFace', { faceId: memberFace.faceId, error: result.error?.message }), 'error');
    }
  }

  if (allSucceeded) {
    showSnackbar(t('memberFace.messages.addSuccess'), 'success');
    faceStore.resetState(); // Reset face store after saving
    if (faceUploadInputRef.value) {
      faceUploadInputRef.value.reset(); // Clear the file input
    }
    emit('saved');
  } else {
    // A generic error message if some failed but not all
    showSnackbar(t('memberFace.messages.partialSaveError'), 'warning');
  }
};

const closeForm = () => {
  faceStore.resetState(); // Clear face detection state on close
  emit('close');
};
</script>
