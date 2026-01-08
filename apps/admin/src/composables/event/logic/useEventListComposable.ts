import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventDto } from '@/types';
import type { DataTableHeader } from 'vuetify';

import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { formatDate } from '@/utils/dateUtils';
// REMOVED: import { useAuthStore } from '@/stores/auth.store'; // No longer needed
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
// REMOVED: import { useEventService } from '@/services/event.service'; // No longer needed

export function useEventListComposable(props: {
  events: EventDto[];
  totalEvents: number;
  loading: boolean;
  search: string;
  familyId?: string; // familyId prop added
}, emit: (event: 'update:options' | 'view' | 'edit' | 'delete' | 'create' | 'update:search', ...args: any[]) => void) {
  const { t } = useI18n();
  // REMOVED: const authStore = useAuthStore(); // No longer needed
  const { showSnackbar } = useGlobalSnackbar();
  // REMOVED: const eventService = useEventService(); // No longer needed

  const searchQuery = ref(props.search);
  let debounceTimer: ReturnType<typeof setTimeout> | undefined;

  const debouncedSearch = computed({
    get() {
      return searchQuery.value;
    },
    set(newValue: string) {
      searchQuery.value = newValue;
      if (debounceTimer) {
        clearTimeout(debounceTimer);
      }
      debounceTimer = setTimeout(() => {
        emit('update:search', newValue);
      }, 300);
    },
  });

  const handlePropsSearchChange = (newSearch: string) => {
    if (newSearch !== searchQuery.value) {
      searchQuery.value = newSearch;
    }
  };

  watch(() => props.search, handlePropsSearchChange);

  const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

  const headers = computed<DataTableHeader[]>(() => [
    {
      title: t('event.list.headers.date'),
      key: 'date',
      width: '120px',
      align: 'center',
    },
    {
      title: t('event.list.headers.currentYearOccurrenceDate'),
      key: 'currentYearOccurrenceDate',
      width: '150px',
      align: 'center',
      sortable: false,
    },
    {
      title: t('event.list.headers.name'),
      key: 'name',
      minWidth: '150px',
      align: 'start',
      class: 'text-truncate',
    },
    {
      title: t('event.list.headers.family'),
      key: 'familyId',
      width: '150px',
      align: 'start',
      sortable: false,
    },
    {
      title: t('event.list.headers.relatedMembers'),
      key: 'relatedMembers',
      width: 'auto',
      align: 'start',
      sortable: false,
    },
    {
      title: t('event.list.headers.actions'),
      key: 'actions',
      sortable: false,
      width: '120px',
      align: 'center',
    },
  ]);

  const loadEvents = (options: {
    page: number;
    itemsPerPage: number;
    sortBy: { key: string; order: string }[];
  }) => {
    emit('update:options', options);
  };

  const editEvent = (eventId: string) => {
    emit('edit', eventId);
  };

  const confirmDelete = (eventId: string) => {
    emit('delete', eventId);
  };

  // REMOVED: isGeneratingOccurrences and generateEventOccurrences
  // const isGeneratingOccurrences = ref(false);
  // const generateEventOccurrences = async (year: number) => { /* ... */ };

  return {
    state: {
      debouncedSearch,
      itemsPerPage,
      headers,
      // REMOVED: isGeneratingOccurrences,
    },
    actions: {
      t,
      loadEvents,
      editEvent,
      confirmDelete,
      formatDate,
      // REMOVED: generateEventOccurrences,
    },
  };
}
