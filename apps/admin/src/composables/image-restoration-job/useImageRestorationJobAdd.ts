import { ref, reactive, type Ref } from 'vue';
import { useCreateImageRestorationJobMutation } from './useCreateImageRestorationJobMutation';
import { type IImageRestorationJobFormInstance } from '@/components/image-restoration-job/ImageRestorationJobForm.vue';
import { type CreateImageRestorationJobDto } from '@/types';

// Helper function for artificial delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

interface UseImageRestorationJobAddOptions {
  familyId: string;
  onSaveSuccess?: () => void;
  onCancel?: () => void;
  formRef: Ref<IImageRestorationJobFormInstance | null>;
}

export const useImageRestorationJobAdd = (options: UseImageRestorationJobAddOptions) => {
  const { familyId, onSaveSuccess, onCancel, formRef } = options;
  const isAddingImageRestorationJob = ref(false);

  const { mutateAsync: createImageRestorationJob } = useCreateImageRestorationJobMutation();

  const handleAddItem = async () => {
    if (!formRef.value) return;

    const { valid } = await formRef.value.validate();
    if (!valid) return;

    isAddingImageRestorationJob.value = true;
    try {
      const formData = formRef.value.getFormData();
      const command: CreateImageRestorationJobDto = {
        originalImageUrl: formData.originalImageUrl,
        familyId: familyId,
      };
      await createImageRestorationJob(command);
      onSaveSuccess?.();
      formRef.value.reset();
    } finally {
      await delay(300); // Artificial delay to make loading state more visible
      isAddingImageRestorationJob.value = false;
    }
  };

  const closeForm = () => {
    onCancel?.();
    formRef.value?.reset();
  };

  return {
    state: reactive({
      isAddingImageRestorationJob,
    }),
    actions: {
      handleAddItem,
      closeForm,
    },
  };
};