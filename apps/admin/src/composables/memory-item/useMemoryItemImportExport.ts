import { type Ref } from 'vue';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import { downloadFile } from '@/utils/file-helpers';
import { useServices } from '@/plugins/services.plugin';
import { useMutation } from '@tanstack/vue-query'; // Import useMutation

export function useMemoryItemImportExport(familyId: Ref<string | undefined>) {
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const { memoryItem } = useServices();

  const exportMemoryItemsMutation = useMutation({
    mutationFn: async () => {
      const result = await memoryItem.exportMemoryItems(familyId.value);
      if (result.ok && result.value) {
        downloadFile(JSON.stringify(result.value), `memory-items-${familyId.value || 'all'}.json`, 'application/json');
        showSnackbar(t('memoryItem.messages.exportSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('memoryItem.messages.exportError'), 'error');
        throw new Error(result.error?.message || t('memoryItem.messages.exportError')); // Throw error to be caught by onError
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('memoryItem.messages.exportError'), 'error');
    },
  });

  const importMemoryItemsMutation = useMutation({
    mutationFn: async (jsonData: any) => {
      if (!familyId.value) {
        showSnackbar(t('memoryItem.messages.noFamilyId'), 'error');
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      const result = await memoryItem.importMemoryItems(familyId.value, jsonData);
      if (result.ok) {
        showSnackbar(t('memoryItem.messages.importSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('memoryItem.messages.importError'), 'error');
        throw new Error(result.error?.message || t('memoryItem.messages.importError')); // Throw error to be caught by onError
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('memoryItem.messages.importError'), 'error');
    },
  });

  return {
    isExporting: exportMemoryItemsMutation.isPending,
    exportMemoryItems: exportMemoryItemsMutation.mutateAsync,
    isImporting: importMemoryItemsMutation.isPending,
    importMemoryItems: importMemoryItemsMutation.mutateAsync,
  };
}
