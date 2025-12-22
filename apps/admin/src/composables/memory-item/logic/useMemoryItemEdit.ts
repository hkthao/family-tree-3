import { computed, type Ref, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useMemoryItemQuery, useUpdateMemoryItemMutation, useAddFamilyMediaMutation } from '@/composables';
import type { MemoryItem, FamilyMedia, MemoryMedia } from '@/types';
import type { LocalMemoryMedia } from '@/composables/memory-item/useMemoryItemForm'; // Import LocalMemoryMedia
import type { IMemoryItemFormInstance } from '@/components/memory-item/MemoryItemForm.vue'; // Import the exposed interface

interface UseMemoryItemEditOptions {
  familyId: string;
  memoryItemId: string;
  onSaveSuccess: () => void;
  onCancel: () => void;
  formRef: Ref<IMemoryItemFormInstance | null>;
}

export function useMemoryItemEdit(options: UseMemoryItemEditOptions) {
  const { familyId, memoryItemId, onSaveSuccess, onCancel, formRef } = options;

  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const isUploadingMedia = ref(false);

  const { data: memoryItem, isLoading: isLoadingMemoryItem, refetch } = useMemoryItemQuery(
    familyId,
    memoryItemId,
  );
  const { mutate: updateMemoryItem, isPending: isUpdatingMemoryItem } = useUpdateMemoryItemMutation();
  const { mutateAsync: addFamilyMedia } = useAddFamilyMediaMutation();
  const isLoading = computed(() => isLoadingMemoryItem.value);

  const handleUpdateItem = async () => {
    if (!formRef.value) return;
    const isValid = await formRef.value.validate();
    if (!isValid) return;

    const itemData = formRef.value.getFormData();
    const newlyUploadedFiles = formRef.value.newlyUploadedFiles.value; // Access the array value
    const deletedMediaIds = formRef.value.deletedMediaIds;

    // Handle media upload
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
      memoryItemId: itemData.id || '',
      url: media.filePath,
      type: media.mediaType, // Map media.mediaType to type
    }));

    itemData.memoryMedia = [...(itemData.memoryMedia || []), ...newMemoryMedia];
    itemData.deletedMediaIds = deletedMediaIds.value; // Assign deleted media IDs

    if (!itemData.id) {
      showSnackbar(
        t('memoryItem.messages.saveError'),
        'error',
      );
      return;
    }

    updateMemoryItem(itemData, {
      onSuccess: () => {
        showSnackbar(
          t('memoryItem.messages.updateSuccess'),
          'success',
        );
        refetch(); // Refresh the data after successful update
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

  const closeForm = () => {
    onCancel();
  };

  return {
    state: {
      memoryItem,
      isLoading,
      isUpdatingMemoryItem,
      isUploadingMedia,
    },
    actions: {
      handleUpdateItem,
      closeForm,
    },
  };
}