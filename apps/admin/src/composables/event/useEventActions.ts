import { ref, computed, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useAuthStore } from '@/stores/auth.store';
import { useEventService } from '@/services/event.service';
import { useEventImportExport } from './useEventImportExport'; // Assuming useEventImportExport is in the same directory

import type { QueryObserverResult } from '@tanstack/vue-query';
import type { Paginated, EventDto, GenerateAndNotifyEventsCommand } from '@/types'; // NEW GenerateAndNotifyEventsCommand

// ... (other imports)

export const useEventActions = (
  props: any,
  emit: (event: 'close' | 'saved', ...args: any[]) => void,
  refetchEvents: (options?: any) => Promise<QueryObserverResult<Paginated<EventDto>, Error>>
) => {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const authStore = useAuthStore();
  const eventService = useEventService();

  const importDialog = ref(false);
  const isGeneratingOccurrences = ref(false);
  const isSendingNotification = ref(false);
  const isGeneratingAndNotifying = ref(false); // NEW

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

  const triggerGenerateAndNotify = async (currentFamilyId?: string) => {
    isGeneratingAndNotifying.value = true;
    try {
      const command: GenerateAndNotifyEventsCommand = {
        familyId: currentFamilyId,
        year: new Date().getFullYear(), // Use current year as default
      };
      const result = await eventService.generateAndNotifyEvents(command);
      if (result.ok) {
        showSnackbar(t('event.messages.generateAndNotifySuccess'), 'success');
        refetchEvents(); // Refetch events as occurrences might have been generated
      } else {
        showSnackbar(result.error?.message || t('event.messages.generateAndNotifyError'), 'error');
      }
    } catch (error: any) {
      console.error('Error generating and notifying events:', error);
      showSnackbar(error.message || t('event.messages.generateAndNotifyError'), 'error');
    } finally {
      isGeneratingAndNotifying.value = false;
    }
  };

  const isAdmin = computed(() => authStore.isAdmin);

  return {
    importDialog,
    isExporting,
    isImporting,
    isGeneratingOccurrences,
    isSendingNotification,
    isGeneratingAndNotifying, // NEW
    isAdmin,
    exportEvents,
    triggerImport,
    handleGenerateOccurrences,
    handleSendNotification,
    handleGenerateAndNotify: triggerGenerateAndNotify, // NEW
  };
};
