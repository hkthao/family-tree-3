import { ref, computed, watch } from 'vue';
import type { DetectedFace, MemberFace } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useDetectFacesMutation, useAddMemberFaceMutation } from '@/composables/member-face/mutations';

interface UseMemberFaceAddOptions {
  familyId?: string;
  memberId?: string;
  onSaved?: () => void;
  onClosed?: () => void;
  t: (key: string) => string; // Add t function to options
}

export function useMemberFaceAdd(options: UseMemberFaceAddOptions) {
  const { showSnackbar } = useGlobalSnackbar();

  const { mutate: detectFaces, isPending: isDetectingFaces, error: detectError } = useDetectFacesMutation();
  const { mutateAsync: addMemberFace, isPending: isAddingMemberFace, error: addError } = useAddMemberFaceMutation();

  const showSelectMemberDialog = ref(false);
  const faceToLabel = ref<DetectedFace | null>(null);
  const selectedFamilyId = ref<string | undefined>(options.familyId);

  const uploadedImage = ref<string | null | undefined>(undefined); // Base64 string of the uploaded image
  const detectedFaces = ref<DetectedFace[]>([]);
  const originalImageUrl = ref<string | null>(null);

  watch(() => options.familyId, (newFamilyId) => {
    selectedFamilyId.value = newFamilyId;
  });

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
  };

  const handleFileUpload = async (file: File | File[] | null) => {
    if (!file) {
      resetState();
      return;
    }

    detectedFaces.value = [];
    originalImageUrl.value = null;

    const fileToUpload = Array.isArray(file) ? file[0] : file;

    const reader = new FileReader();
    reader.onload = (e) => {
      uploadedImage.value = e.target?.result as string;
    };
    reader.readAsDataURL(fileToUpload);

    detectFaces({ imageFile: fileToUpload, familyId: selectedFamilyId.value!, resize: false }, {
      onSuccess: (data) => {
        detectedFaces.value = data.detectedFaces;
        originalImageUrl.value = data.originalImageUrl;
      },
      onError: (error) => {
        showSnackbar(error.message, 'error');
        uploadedImage.value = null;
        detectedFaces.value = [];
      },
    });
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
      detectedFaces.value.splice(index, 1, updatedFace);
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
      showSnackbar(options.t('memberFace.messages.noFamilySelected'), 'warning');
      return;
    }

    const facesToSave = detectedFaces.value
      .filter(face => face.memberId)
      .map(face => {
        const memberFace: Omit<MemberFace, 'id'> = {
          memberId: face.memberId!,
          faceId: face.id,
          boundingBox: face.boundingBox,
          confidence: face.confidence,
          thumbnail: face.thumbnail,
          thumbnailUrl: face.thumbnailUrl,
          originalImageUrl: originalImageUrl.value,
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
      showSnackbar(options.t('memberFace.messages.noFacesToSave'), 'warning');
      return;
    }

    const results = await Promise.allSettled(
      facesToSave.map(memberFace => {
        if (options.memberId) {
          memberFace.memberId = options.memberId;
        }
        return addMemberFace(memberFace);
      })
    );

    const successfulSaves = results.filter(result => result.status === 'fulfilled').length;
    const failedSaves = results.filter(result => result.status === 'rejected').length;

    if (successfulSaves > 0 && failedSaves === 0) {
      showSnackbar(options.t('memberFace.messages.addSuccess'), 'success');
      resetState();
      options.onSaved?.();
    } else if (successfulSaves > 0 && failedSaves > 0) {
      showSnackbar(options.t('memberFace.messages.partialSaveError'), 'warning');
    } else {
      showSnackbar(options.t('memberFace.messages.saveError'), 'error');
    }
  };

  const closeForm = () => {
    resetState();
    options.onClosed?.();
  };

  return {
    isDetectingFaces,
    isAddingMemberFace,
    showSelectMemberDialog,
    faceToLabel,
    selectedFamilyId,
    uploadedImage,
    detectedFaces,
    canSaveLabels,
    handleFileUpload,
    openSelectMemberDialog,
    handleLabelFaceAndCloseDialog,
    handleRemoveFace,
    saveAllLabeledFaces,
    closeForm,
    resetState,
  };
}