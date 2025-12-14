import { computed, type ComputedRef } from 'vue';
import type { MemberStoryDto } from '@/types/memberStory';
// Removed useGlobalSnackbar and useI18n, type DetectedFace
// Removed FaceDetectionRessult

interface UseMemberStoryFormOptions {
  modelValue: ComputedRef<MemberStoryDto>;
  readonly?: boolean;
  memberId?: string | null;
  familyId?: string;
  updateModelValue: (payload: Partial<MemberStoryDto>) => void;
}

export function useMemberStoryForm(options: UseMemberStoryFormOptions) {
  const { modelValue } = options; // readonly, updateModelValue not directly used here anymore

  const hasUploadedImage = computed(() => {
    return !!modelValue.value.temporaryOriginalImageUrl || (modelValue.value.memberStoryImages && modelValue.value.memberStoryImages.length > 0);
  });

  const isLoading = computed(() => false);

  return {
    hasUploadedImage,
    isLoading,
    // handleFileUpload is removed
  };
}