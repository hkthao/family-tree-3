import { ref, computed, watch } from 'vue';
import type { DetectedFace, MemberFace, FamilyMedia } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useDetectFacesMutation, useAddMemberFaceMutation } from '@/composables/member-face/mutations';
import { useAddFamilyMediaMutation } from '@/composables/family-media';
import { dataURLtoFile } from '@/utils/file'; // Import the new utility

interface UseMemberFaceAddOptions {
  familyId?: string;
  memberId?: string;
  onSaved?: () => void;
  onClosed?: () => void;
  t: (key: string) => string; // Add t function to options
}

export function useMemberFaceAdd(options: UseMemberFaceAddOptions) {
  const { showSnackbar }: { showSnackbar: (message: string, color?: string, timeout?: number) => void } = useGlobalSnackbar();

  const { mutate: detectFaces, isPending: isDetectingFaces, error: detectError } = useDetectFacesMutation();
  const { mutateAsync: addMemberFace, isPending: isAddingMemberFace, error: addError } = useAddMemberFaceMutation();
  const { mutateAsync: addFamilyMedia, isPending: isAddingFamilyMedia } = useAddFamilyMediaMutation();

  const showSelectMemberDialog = ref(false);
  const faceToLabel = ref<DetectedFace | null>(null);
  const selectedFamilyId = ref<string | undefined>(options.familyId);

  const uploadedImage = ref<string | null | undefined>(undefined);
  const uploadedFile = ref<File | null>(null);
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
    uploadedFile.value = null;
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
    uploadedFile.value = fileToUpload;

    const reader = new FileReader();
    reader.onload = (e) => {
      uploadedImage.value = e.target?.result as string;
    };
    reader.readAsDataURL(fileToUpload);

    detectFaces({ imageFile: fileToUpload, familyId: selectedFamilyId.value!, resize: false }, {
      onSuccess: (data) => {
        detectedFaces.value = data.detectedFaces;
      },
      onError: (error) => {
        showSnackbar(error.message, 'error');
        uploadedImage.value = null;
        uploadedFile.value = null;
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

    if (!uploadedFile.value) {
      showSnackbar(options.t('memberFace.messages.noImageUploaded'), 'error');
      return;
    }

    // Step 1: Upload the original image as FamilyMedia
    let mediaCreationResult;
    try {
      mediaCreationResult = await addFamilyMedia({
        familyId: selectedFamilyId.value!,
        file: uploadedFile.value,
        description: options.t('memberFace.messages.uploadedImageDescription'), // Provide a generic description
      });
      originalImageUrl.value = mediaCreationResult.filePath; // Store the permanent URL
    } catch (error: any) {
      showSnackbar(error.message || options.t('memberFace.messages.uploadImageFailed'), 'error');
      return;
    }

    const thumbnailUploadPromises = detectedFaces.value
      .filter(face => face.thumbnail && face.memberId)
      .map(async (face) => {
        if (!face.thumbnail) {
          // This case should ideally be caught by the filter, but for type safety
          throw new Error('Thumbnail not found despite filter.');
        }
        const thumbnailFile = dataURLtoFile(`data:image/png;base64,${face.thumbnail}`, `thumbnail-${face.id}.png`); const thumbnailMediaResult = await addFamilyMedia({
          familyId: selectedFamilyId.value!,
          file: thumbnailFile
        });
        return { face, filePath: thumbnailMediaResult.filePath };
      });

    let uploadedThumbnailPaths: { face: DetectedFace; filePath: string }[];
    try {
      uploadedThumbnailPaths = await Promise.all(thumbnailUploadPromises);
    } catch (error: any) {
      const errorMessage = options.t('memberFace.messages.uploadThumbnailFailed') + (error.message || error.toString() || options.t('common.unknown'));
      console.error('Failed to upload one or more thumbnails:', error);
      showSnackbar(errorMessage, 'error');
      return; // Stop the entire process
    }

    uploadedThumbnailPaths.forEach(item => {
      const originalFace = detectedFaces.value.find(f => f.id === item.face.id);
      if (originalFace) {
        originalFace.thumbnailUrl = item.filePath;
      }
    });

    const facesToSave = detectedFaces.value
      .filter(face => face.memberId)
      .map(face => {
        const memberFace: Omit<MemberFace, 'id'> = {
          memberId: face.memberId!,
          faceId: face.id,
          boundingBox: face.boundingBox,
          confidence: face.confidence,
          thumbnail: face.thumbnail,
          thumbnailUrl: face.thumbnailUrl, // Now this should be the permanent URL
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

    const memberFaceSaveResults = await Promise.allSettled(
      facesToSave.map(memberFace => {
        if (options.memberId) {
          memberFace.memberId = options.memberId;
        }
        return addMemberFace(memberFace);
      })
    );

    const successfulSaves = memberFaceSaveResults.filter(result => result.status === 'fulfilled').length;
    const failedSaves = memberFaceSaveResults.filter(result => result.status === 'rejected').length;

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

  const isSaving = computed(() => isAddingMemberFace.value || isAddingFamilyMedia.value);

  const closeForm = () => {
    resetState();
    options.onClosed?.();
  };

  return {
    isDetectingFaces,
    isAddingMemberFace,
    isSaving,
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