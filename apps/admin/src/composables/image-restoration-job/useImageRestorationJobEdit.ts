import { ref, watch, reactive, computed, type Ref } from 'vue';
import { useImageRestorationJobQuery } from './useImageRestorationJobQuery';
import { useUpdateImageRestorationJobMutation } from './useUpdateImageRestorationJobMutation';
import { type IImageRestorationJobFormInstance } from '@/components/image-restoration-job/ImageRestorationJobForm.vue';
import { type ImageRestorationJobDto, type UpdateImageRestorationJobCommand } from '@/types';

interface UseImageRestorationJobEditOptions {
  familyId: Ref<string>;
  imageRestorationJobId: Ref<string>;
  onSaveSuccess?: () => void;
  onCancel?: () => void;
  formRef: Ref<IImageRestorationJobFormInstance | null>;
}

export const useImageRestorationJobEdit = (options: UseImageRestorationJobEditOptions) => {
  const { familyId, imageRestorationJobId, onSaveSuccess, onCancel, formRef } = options;

  const { state: { imageRestorationJob, isLoading, error } } = useImageRestorationJobQuery(familyId, imageRestorationJobId);
  const { mutateAsync: updateImageRestorationJob, isPending: isUpdatingImageRestorationJob } = useUpdateImageRestorationJobMutation();

  watch(imageRestorationJob, (newVal) => {
    if (newVal && formRef.value) {
      formRef.value.getFormData().originalImageUrl = newVal.originalImageUrl; // Update form data
      // Note: Other fields like status, restoredImageUrl, errorMessage are read-only in form or updated by backend process
    }
  });

  const handleUpdateItem = async () => {
    if (!formRef.value) return;

    const { valid } = await formRef.value.validate();
    if (!valid) return;

    try {
      const formData = formRef.value.getFormData();
      const command: UpdateImageRestorationJobCommand = {
        jobId: imageRestorationJobId.value,
      };
      await updateImageRestorationJob(command);
      onSaveSuccess?.();
    } finally {
      // isUpdatingImageRestorationJob handles loading state
    }
  };

  const closeForm = () => {
    onCancel?.();
  };

  return {
    state: reactive({
      imageRestorationJob,
      isLoading,
      error,
      isUpdatingImageRestorationJob,
    }),
    actions: {
      handleUpdateItem,
      closeForm,
    },
  };
};