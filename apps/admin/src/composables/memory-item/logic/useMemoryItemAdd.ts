import { ref, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useAddMemoryItemMutation } from '@/composables';
import { useAddFamilyMediaMutation } from '@/composables';
import type { MemoryItemFormExpose } from '@/components/memory-item/MemoryItemForm.vue';
import type { MemoryItem, FamilyMedia, MemoryMedia } from '@/types';

interface UseMemoryItemAddOptions {
  familyId: string;
  onSaveSuccess: () => void; // Callback for successful save
  onCancel: () => void;      // Callback for cancel
  formRef: Ref<MemoryItemFormExpose | null>; // Pass the ref from the component
}

export function useMemoryItemAdd(options: UseMemoryItemAddOptions) {
  const { familyId, onSaveSuccess, onCancel, formRef } = options;

  const isUploadingMedia = ref(false);

  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { mutate: addMemoryItem, isPending: isAddingMemoryItem } = useAddMemoryItemMutation();
  const { mutateAsync: addFamilyMedia } = useAddFamilyMediaMutation();

  const handleAddItem = async () => {
    if (!formRef.value) return;
    const isValid = await formRef.value.validate();
    if (!isValid) return;

    const itemData = formRef.value.getFormData();
    const newlyUploadedFiles = formRef.value.newlyUploadedFiles.value;

    const uploadedMedia: FamilyMedia[] = [];

    if (newlyUploadedFiles && newlyUploadedFiles.length > 0) {
      isUploadingMedia.value = true;
      try {
        for (const file of newlyUploadedFiles) {
          const media = await addFamilyMedia({ familyId: familyId, file: file });
          uploadedMedia.push(media);
        }
        showSnackbar(t('familyMedia.messages.uploadSuccess'), 'success');
      } catch (error: any) {
        showSnackbar(error.message || t('familyMedia.messages.uploadError'), 'error');
        isUploadingMedia.value = false;
        return;
      } finally {
        isUploadingMedia.value = false;
      }
    }

    // Append newly uploaded media to the existing media in itemData
    const newMemoryMedia: MemoryMedia[] = uploadedMedia.map(media => ({
      id: media.id,
      memoryItemId: '', // Will be assigned on save
      url: media.filePath,
    }));

    itemData.memoryMedia = [...(itemData.memoryMedia || []), ...newMemoryMedia];

    addMemoryItem(itemData as Omit<MemoryItem, 'id'>, {
      onSuccess: () => {
        showSnackbar(
          t('memoryItem.messages.addSuccess'),
          'success',
        );
        onSaveSuccess();
      },
      onError: (error) => {
        showSnackbar(
          error.message || t('memoryItem.messages.saveError'),
          'error',
        );
      },
    });
  };

  return {
    isUploadingMedia,
    isAddingMemoryItem,
    handleAddItem,
    closeForm: onCancel,
  };
}
