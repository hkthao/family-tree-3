import { computed, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useMemoryItemQuery, useUpdateMemoryItemMutation } from '@/composables';
import type { IMemoryItemFormInstance } from '@/components/memory-item/MemoryItemForm.vue'; // Import the exposed interface

interface UseMemoryItemEditOptions {
  familyId: string;
  memoryItemId: string;
  onSaveSuccess: () => void;
  onCancel: () => void;
  formRef: Ref<IMemoryItemFormInstance | null>;
}

export function useMemoryItemEdit(options: UseMemoryItemEditOptions) {
  const { memoryItemId, onSaveSuccess, onCancel, formRef } = options;

  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();



  const { data: memoryItem, isLoading: isLoadingMemoryItem, refetch } = useMemoryItemQuery(
    memoryItemId,
  );
  const { mutate: updateMemoryItem, isPending: isUpdatingMemoryItem } = useUpdateMemoryItemMutation();

  const isLoading = computed(() => isLoadingMemoryItem.value);

  const handleUpdateItem = async () => {
    if (!formRef.value) return;
    const isValid = await formRef.value.validate();
    if (!isValid) return;

    const itemData = formRef.value.getFormData();

    const deletedMediaIds = formRef.value.deletedMediaIds; // Still needed


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

    },
    actions: {
      handleUpdateItem,
      closeForm,
    },
  };
}