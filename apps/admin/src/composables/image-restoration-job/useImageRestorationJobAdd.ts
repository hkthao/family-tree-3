import { reactive, computed } from 'vue';
import { useCreateImageRestorationJobMutation } from './useCreateImageRestorationJobMutation';
// import { type IImageRestorationJobFormInstance } from '@/components/image-restoration-job/ImageRestorationJobForm.vue'; // Removed
// import { type CreateImageRestorationJobDto } from '@/types'; // Removed

interface UseImageRestorationJobAddOptions {
  familyId: string;
  onSaveSuccess?: () => void;
  onCancel?: () => void;
  // formRef: Ref<IImageRestorationJobFormInstance | null>; // Removed
}

export const useImageRestorationJobAdd = (options: UseImageRestorationJobAddOptions) => {
  const { familyId, onSaveSuccess, onCancel } = options;

  const { mutateAsync: createImageRestorationJob, isPending } = useCreateImageRestorationJobMutation();

  const handleAddItem = async (file: File, useCodeformer: boolean) => {
    try {
      await createImageRestorationJob({ file, familyId, useCodeformer }); // Pass new parameters
      onSaveSuccess?.();
    } finally {
      // isPending state directly from useMutation will handle loading, no artificial delay needed
    }
  };

  const closeForm = () => {
    onCancel?.();
  };

  return {
    state: reactive({
      isAddingImageRestorationJob: computed(() => isPending.value), // Use computed to maintain reactivity
    }),
    actions: {
      handleAddItem,
      closeForm,
    },
    internal: { // Expose handleAddItem under internal for renaming in ImageRestorationJobAddView.vue
      handleAddItem,
    }
  };
};