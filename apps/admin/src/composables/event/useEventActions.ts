import { ref, computed, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useAuthStore } from '@/stores/auth.store';
import { useEventService } from '@/services/event.service';
import { useEventImportExport } from './useEventImportExport'; // Assuming useEventImportExport is in the same directory

export const useEventActions = (
  props: any,
  emit: (event: string, ...args: any[]) => void,
  refetchEvents: () => void | Promise<void>
) => {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const authStore = useAuthStore();
  const eventService = useEventService();

  const importDialog = ref(false);
  const isGeneratingOccurrences = ref(false);
  const isSendingNotification = ref(false);

  // Extract familyId from props, ensuring it's reactive if props are reactive
  const { familyId } = toRefs(props);

  const { isExporting, isImporting, exportEvents, importEvents } = useEventImportExport(familyId);

  const triggerImport = async (file: File) => {
    if (!file) {
      showSnackbar(t('event.messages.noFileSelected'), 'warning');
      return;
    }

    const reader = new FileReader();
    reader.onload = async (e) => {
      try {
        const jsonContent = JSON.parse(e.target?.result as string);
        await importEvents(jsonContent);
        importDialog.value = false;
        refetchEvents();
      } catch (error: any) {
        console.error("Import operation failed:", error);
        showSnackbar(t('event.messages.importError'), 'error'); // Show generic import error
      }
    };
    reader.readAsText(file);
  };

  const handleGenerateOccurrences = async (year: number, currentFamilyId: string) => {
    isGeneratingOccurrences.value = true;
    const result = await eventService.generateEventOccurrences(year, currentFamilyId);
    if (result.ok) {
      showSnackbar(t('event.list.action.generateOccurrencesSuccess'), 'success');
      refetchEvents(); // Reload events
    } else {
      showSnackbar(result.error?.message || t('event.list.action.generateOccurrencesError'), 'error');
    }
    isGeneratingOccurrences.value = false;
  };

  const handleSendNotification = async (eventId: string) => {
    isSendingNotification.value = true;
    const result = await eventService.sendEventNotification(eventId);
    if (result.ok) {
      showSnackbar(t('event.messages.sendNotificationSuccess'), 'success');
      // No refetchEvents needed here as sending notification doesn't change event data
    } else {
      showSnackbar(result.error?.message || t('event.messages.sendNotificationError'), 'error');
    }
    isSendingNotification.value = false;
  };

  const isAdmin = computed(() => authStore.isAdmin);

  return {
    importDialog,
    isExporting,
    isImporting,
    isGeneratingOccurrences,
    isSendingNotification,
    isAdmin,
    exportEvents,
    triggerImport,
    handleGenerateOccurrences,
    handleSendNotification,
  };
};
