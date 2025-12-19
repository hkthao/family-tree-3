import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import type { DataTableHeader } from 'vuetify';

import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { formatDate } from '@/utils/dateUtils';

export function useEventListComposable(props: {
  events: Event[];
  totalEvents: number;
  loading: boolean;
  search: string;
}, emit: (event: 'update:options' | 'view' | 'edit' | 'delete' | 'create' | 'update:search', ...args: any[]) => void) {
  const { t } = useI18n();

  const searchQuery = ref(props.search);
  let debounceTimer: ReturnType<typeof setTimeout>;

  const debouncedSearch = computed({
    get() {
      return searchQuery.value;
    },
    set(newValue: string) {
      searchQuery.value = newValue;
      clearTimeout(debounceTimer);
      debounceTimer = setTimeout(() => {
        emit('update:search', newValue);
      }, 300);
    },
  });

  watch(() => props.search, (newSearch) => {
    if (newSearch !== searchQuery.value) {
      searchQuery.value = newSearch;
    }
  });

  const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

  const headers = computed<DataTableHeader[]>(() => [
    {
      title: t('event.list.headers.date'),
      key: 'date',
      width: '120px',
      align: 'center',
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

  return {
    t,
    debouncedSearch,
    itemsPerPage,
    headers,
    loadEvents,
    editEvent,
    confirmDelete,
    formatDate,
  };
}