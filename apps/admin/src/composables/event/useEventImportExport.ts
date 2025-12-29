import { type Ref } from 'vue';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import { downloadFile } from '@/utils/file-helpers';
import { useServices } from '@/plugins/services.plugin';
import { useMutation } from '@tanstack/vue-query';

export function useEventImportExport(familyId: Ref<string | undefined>) {
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const { event: eventService } = useServices(); // Renamed to avoid conflict with event variable

  const exportEventsMutation = useMutation({
    mutationFn: async () => {
      const result = await eventService.exportEvents(familyId.value);
      if (result.ok && result.value) {
        downloadFile(result.value, `events-${familyId.value || 'all'}.json`, 'application/json');
        showSnackbar(t('event.messages.exportSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('event.messages.exportError'), 'error');
        throw new Error(result.error?.message || t('event.messages.exportError'));
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('event.messages.exportError'), 'error');
    },
  });

  const importEventsMutation = useMutation({
    mutationFn: async (jsonData: any) => {
      if (!familyId.value) {
        showSnackbar(t('event.messages.noFamilyId'), 'error');
        throw new Error(t('event.messages.noFamilyId'));
      }
      const result = await eventService.importEvents(familyId.value, jsonData);
      if (result.ok) {
        showSnackbar(t('event.messages.importSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('event.messages.importError'), 'error');
        throw new Error(result.error?.message || t('event.messages.importError'));
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('event.messages.importError'), 'error');
    },
  });

  return {
    isExporting: exportEventsMutation.isPending,
    exportEvents: exportEventsMutation.mutateAsync,
    isImporting: importEventsMutation.isPending,
    importEvents: importEventsMutation.mutateAsync,
  };
}
