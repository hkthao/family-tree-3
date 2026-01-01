import { ref, reactive, type Ref } from 'vue';
import { useCreateImageRestorationJobMutation } from './useCreateImageRestorationJobMutation';
import { type IImageRestorationJobFormInstance } from '@/components/image-restoration-job/ImageRestorationJobForm.vue';
import { type ImageRestorationJobDto, type CreateImageRestorationJobCommand } from '@/types';

interface UseImageRestorationJobAddOptions {
  familyId: string;
  onSaveSuccess?: () => void;
  onCancel?: () => void;
  formRef: Ref<IImageRestorationJobFormInstance | null>;
}

export const useImageRestorationJobAdd = (options: UseImageRestorationJobAddOptions) => {
  const { familyId, onSaveSuccess, onCancel, formRef } = options;
  const isAddingImageRestorationJob = ref(false); // Renamed from isAddingMemoryItem

  const { mutateAsync: createImageRestorationJob } = useCreateImageRestorationJobMutation();

  const handleAddItem = async () => {
    if (!formRef.value) return;

    const { valid } = await formRef.value.validate();
    if (!valid) return;

    isAddingImageRestorationJob.value = true;
    try {
      const formData = formRef.value.getFormData();
      const command: CreateImageRestorationJobCommand = {
        originalImageUrl: formData.originalImageUrl,
        familyId: familyId,
      };
      await createImageRestorationJob(command);
      onSaveSuccess?.();
      formRef.value.reset();
    } finally {
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