import { ref, reactive } from 'vue';
import { useCreateImageRestorationJobMutation } from './useCreateImageRestorationJobMutation';
// import { type IImageRestorationJobFormInstance } from '@/components/image-restoration-job/ImageRestorationJobForm.vue'; // Removed
// import { type CreateImageRestorationJobDto } from '@/types'; // Removed

// Helper function for artificial delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

interface UseImageRestorationJobAddOptions {
  familyId: string;
  onSaveSuccess?: () => void;
  onCancel?: () => void;
  // formRef: Ref<IImageRestorationJobFormInstance | null>; // Removed
}

export const useImageRestorationJobAdd = (options: UseImageRestorationJobAddOptions) => {
  const { familyId, onSaveSuccess, onCancel } = options;
  const isAddingImageRestorationJob = ref(false);

  const { mutateAsync: createImageRestorationJob } = useCreateImageRestorationJobMutation();

  const handleAddItem = async (file: File, useCodeformer: boolean) => {
    isAddingImageRestorationJob.value = true;
    try {
      await createImageRestorationJob({ file, familyId, useCodeformer }); // Pass new parameters
      onSaveSuccess?.();
    } finally {
      await delay(300); // Artificial delay to make loading state more visible
      isAddingImageRestorationJob.value = false;
    }
  };

  const closeForm = () => {
    onCancel?.();
  };

  return {
    state: reactive({
      isAddingImageRestorationJob,
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