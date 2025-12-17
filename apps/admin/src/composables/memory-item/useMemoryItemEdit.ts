import { ref, computed, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useMemoryItemQuery, useUpdateMemoryItemMutation } from '@/composables/memory-item';
import type { MemoryItem } from '@/types';

interface MemoryItemFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => MemoryItem;
}

interface UseMemoryItemEditOptions {
  familyId: string;
  memoryItemId: string;
  onSaveSuccess: () => void;
  onCancel: () => void;
  formRef: Ref<MemoryItemFormExposed | null>;
}

export function useMemoryItemEdit(options: UseMemoryItemEditOptions) {
  const { familyId, memoryItemId, onSaveSuccess, onCancel, formRef } = options;

  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const { data: memoryItem, isLoading: isLoadingMemoryItem } = useMemoryItemQuery(
    familyId,
    memoryItemId,
  );
  const { mutate: updateMemoryItem, isPending: isUpdatingMemoryItem } = useUpdateMemoryItemMutation();
  const isLoading = computed(() => isLoadingMemoryItem.value);

  const handleUpdateItem = async () => {
    if (!formRef.value) return;
    const isValid = await formRef.value.validate();
    if (!isValid) return;

    const itemData = formRef.value.getFormData();
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
    memoryItem,
    isLoading,
    isUpdatingMemoryItem,
    handleUpdateItem,
    closeForm,
  };
}