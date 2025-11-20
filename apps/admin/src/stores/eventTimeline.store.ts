import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { ApiEventService } from '@/services/event/api.event.service';
import type { Event, EventFilter } from '@/types';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';

export const useEventTimelineStore = defineStore('eventTimeline', () => {
  const eventService = new ApiEventService();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();

  // State
  const events = ref<Event[]>([]);
  const totalEvents = ref(0);
  const currentPage = ref(1);
  const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const filter = ref<EventFilter>({});

  // Getters
  const paginatedEvents = computed(() => events.value);
  const paginationLength = computed(() => {
    if (typeof totalEvents.value !== 'number' || typeof itemsPerPage.value !== 'number' || itemsPerPage.value <= 0) {
      return 1;
    }
    return Math.max(1, Math.ceil(totalEvents.value / itemsPerPage.value));
  });

  // Actions
  const loadEvents = async (familyId?: string, memberId?: string) => {
    loading.value = true;
    error.value = null;
    try {
      const currentFilter: EventFilter = {
        pageNumber: currentPage.value,
        pageSize: itemsPerPage.value,
      };

      if (memberId) {
        currentFilter.relatedMemberId = memberId;
      } else if (familyId) {
        currentFilter.familyId = familyId;
      } else {
        // If no familyId or memberId, clear events and return
        events.value = [];
        totalEvents.value = 0;
        loading.value = false;
        return;
      }

      const result = await eventService.getEvents(currentFilter);

      if (result.isSuccess) {
        events.value = result.value.items;
        totalEvents.value = result.value.totalItems;
      } else {
        error.value = result.error?.message || t('event.timeline.errors.load');
        showSnackbar({
          message: error.value,
          color: 'error',
        });
        events.value = [];
        totalEvents.value = 0;
      }
    } catch (err: any) {
      error.value = err.message || t('event.timeline.errors.load');
      showSnackbar({
        message: error.value,
        color: 'error',
      });
      events.value = [];
      totalEvents.value = 0;
    } finally {
      loading.value = false;
    }
  };

  const setPage = (page: number) => {
    currentPage.value = page;
  };

  const setItemsPerPage = (perPage: number) => {
    itemsPerPage.value = perPage;
  };

  return {
    events,
    totalEvents,
    currentPage,
    itemsPerPage,
    loading,
    error,
    filter,
    paginatedEvents,
    paginationLength,
    loadEvents,
    setPage,
    setItemsPerPage,
  };
});
