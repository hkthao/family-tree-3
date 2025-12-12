<template>
  <v-card :elevation="0" data-testid="member-face-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isDetectingFaces || isAddingMemberFace" indeterminate color="primary" class="my-4"></v-progress-linear>
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
        <v-alert v-else-if="!isDetectingFaces && uploadedImage && detectedFaces.length === 0"
          type="info" class="my-4">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>

      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" :disabled="!canSaveLabels" @click="saveAllLabeledFaces"
        :loading="isAddingMemberFace">
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
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import type { DetectedFace, MemberFace } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useDetectFacesMutation, useAddMemberFaceMutation } from '@/composables/member-face';

interface MemberFaceAddViewProps {
  memberId?: string; // Optional, might be pre-selected if adding from a member detail page
  familyId?: string; // Optional, might be pre-selected if adding from a family context
}

const props = defineProps<MemberFaceAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { mutate: detectFaces, isPending: isDetectingFaces, error: detectError } = useDetectFacesMutation();
const { mutateAsync: addMemberFace, isPending: isAddingMemberFace, error: addError } = useAddMemberFaceMutation(); // Use mutateAsync for awaiting in loop

const showSelectMemberDialog = ref(false);
const faceToLabel = ref<DetectedFace | null>(null);
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null);
const selectedFamilyId = ref<string | undefined>(props.familyId);

const uploadedImage = ref<string | null | undefined>(undefined); // Base64 string of the uploaded image
const detectedFaces = ref<DetectedFace[]>([]);
const originalImageUrl = ref<string | null>(null);

watch(() => props.familyId, (newFamilyId) => {
  selectedFamilyId.value = newFamilyId;
});

// Watch for errors from mutations
watch(detectError, (newError) => {
  if (newError) {
    showSnackbar(newError.message, 'error');
  }
});
watch(addError, (newError) => {
  if (newError) {
    showSnackbar(newError.message, 'error');
  }
});

const resetState = () => {
  uploadedImage.value = null;
  detectedFaces.value = [];
  originalImageUrl.value = null;
  faceToLabel.value = null;
  showSelectMemberDialog.value = false;
  if (faceUploadInputRef.value) {
    faceUploadInputRef.value.reset();
  }
};

const handleFileUpload = async (file: File | File[] | null) => {
  resetState(); // Reset state before new upload
  if (file instanceof File) {
    detectFaces({ imageFile: file, familyId: selectedFamilyId.value!, resize: false }, {
      onSuccess: (data) => {
        uploadedImage.value = data.originalImageBase64;
        detectedFaces.value = data.detectedFaces;
        originalImageUrl.value = data.originalImageUrl;
      },
      onError: (error) => {
        showSnackbar(error.message, 'error');
      },
    });
  } else if (Array.isArray(file) && file.length > 0) {
    detectFaces({ imageFile: file[0], familyId: selectedFamilyId.value!, resize: false }, {
      onSuccess: (data) => {
        uploadedImage.value = data.originalImageBase64;
        detectedFaces.value = data.detectedFaces;
        originalImageUrl.value = data.originalImageUrl;
      },
      onError: (error) => {
        showSnackbar(error.message, 'error');
      },
    });
  }
};

const openSelectMemberDialog = (face: DetectedFace) => {
  if (face.status === 'recognized') {
    return;
  }
  if (face) {
    faceToLabel.value = face;
    showSelectMemberDialog.value = true;
  }
};

const handleLabelFaceAndCloseDialog = (updatedFace: DetectedFace) => {
  const index = detectedFaces.value.findIndex(f => f.id === updatedFace.id);
  if (index !== -1) {
    detectedFaces.value.splice(index, 1, updatedFace); // Replace the old face with the updated one
  }
  showSelectMemberDialog.value = false;
  faceToLabel.value = null;
};

const handleRemoveFace = (faceId: string) => {
  detectedFaces.value = detectedFaces.value.filter(face => face.id !== faceId);
};

const canSaveLabels = computed(() => {
  if (!selectedFamilyId.value) {
    return false;
  }
  const hasFacesToSave = detectedFaces.value.some(face =>
    (face.status === 'labeled') ||
    (face.status === 'unrecognized' && face.memberId !== null) ||
    (face.status === 'recognized' && face.originalMemberId !== face.memberId)
  );

  const hasUnlabeledFacesLeft = detectedFaces.value.some(face =>
    face.status === 'unrecognized' && face.memberId === null
  );

  return hasFacesToSave && !hasUnlabeledFacesLeft;
});

const saveAllLabeledFaces = async () => {
  if (!selectedFamilyId.value) {
    showSnackbar(t('memberFace.messages.noFamilySelected'), 'warning');
    return;
  }
  // Filter out unlabeled faces and transform DetectedFace to MemberFace for creation
  const facesToSave = detectedFaces.value
    .filter(face => face.memberId)
    .map(face => {
      // Create a new MemberFace object from DetectedFace properties
      const memberFace: Omit<MemberFace, 'id'> = {
        memberId: face.memberId!,
        faceId: face.id, // Use the detected face ID as the backend FaceId
        boundingBox: face.boundingBox,
        confidence: face.confidence,
        thumbnail: face.thumbnail,
        thumbnailUrl: face.thumbnailUrl,
        originalImageUrl: originalImageUrl.value, // Use the locally managed original image URL
        embedding: face.embedding || [],
        emotion: face.emotion,
        emotionConfidence: face.emotionConfidence,
        isVectorDbSynced: false,
        vectorDbId: undefined,
        familyId: selectedFamilyId.value!,
      };
      return memberFace;
    });

  if (facesToSave.length === 0) {
    showSnackbar(t('memberFace.messages.noFacesToSave'), 'warning');
    return;
  }

  const results = await Promise.allSettled(
    facesToSave.map(memberFace => {
      if (props.memberId) {
        memberFace.memberId = props.memberId;
      }
      return addMemberFace(memberFace);
    })
  );

  const successfulSaves = results.filter(result => result.status === 'fulfilled').length;
  const failedSaves = results.filter(result => result.status === 'rejected').length;

  if (successfulSaves > 0 && failedSaves === 0) {
    showSnackbar(t('memberFace.messages.addSuccess'), 'success');
    resetState(); // Reset state after saving
    emit('saved');
  } else if (successfulSaves > 0 && failedSaves > 0) {
    showSnackbar(t('memberFace.messages.partialSaveError'), 'warning');
  } else {
    showSnackbar(t('memberFace.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  resetState(); // Clear face detection state on close
  emit('close');
};

defineExpose({
  handleFileUpload,
  saveAllLabeledFaces,
  canSaveLabels,
});
</script>
