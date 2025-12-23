import { ref, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useAddMemoryItemMutation } from '@/composables';
import { useAddFamilyMediaMutation } from '@/composables';
import type { MemoryItem, FamilyMedia, MemoryMedia } from '@/types';
import type { IMemoryItemFormInstance } from '@/components/memory-item/MemoryItemForm.vue'; // Import the exposed interface
import { useQueryClient } from '@tanstack/vue-query'; // Import useQueryClient

interface UseMemoryItemAddOptions {
  familyId: string;
  onSaveSuccess: () => void; // Callback for successful save
  onCancel: () => void;      // Callback for cancel
  formRef: Ref<IMemoryItemFormInstance | null>; // Use the imported interface
}

export function useMemoryItemAdd(options: UseMemoryItemAddOptions) {
  const { familyId, onSaveSuccess, onCancel, formRef } = options;

  const isUploadingMedia = ref(false);

  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient(); // Get queryClient instance
  const { mutate: addMemoryItem, isPending: isAddingMemoryItem } = useAddMemoryItemMutation();
  const { mutateAsync: addFamilyMedia } = useAddFamilyMediaMutation();

  const handleAddItem = async () => {
    if (!formRef.value || typeof formRef.value.validate !== 'function') {
      console.error('Form reference or its validate method is not available.');
      return;
    }

    const isValid = await formRef.value.validate();
    if (!isValid) return;

    const itemData = formRef.value.getFormData();
    const newlyUploadedFiles = formRef.value.newlyUploadedFiles.value; // Access the array value

    const uploadedMedia: FamilyMedia[] = [];

    if (newlyUploadedFiles && newlyUploadedFiles.length > 0) {
      isUploadingMedia.value = true;
      try {
        for (const file of newlyUploadedFiles) { // Iterate over the array value
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
      type: media.mediaType, // Map media.mediaType to type
    }));

    itemData.memoryMedia = [...(itemData.memoryMedia || []), ...newMemoryMedia];

    addMemoryItem(itemData as Omit<MemoryItem, 'id'>, {
      onSuccess: () => {
        showSnackbar(
          t('memoryItem.messages.addSuccess'),
          'success',
        );
        queryClient.invalidateQueries({ queryKey: ['memory-items', { familyId: familyId }] }); // Invalidate the memory-items query
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
    state: {
      isUploadingMedia,
      isAddingMemoryItem,
    },
    actions: {
      handleAddItem,
      closeForm: onCancel,
    },
  };
}
