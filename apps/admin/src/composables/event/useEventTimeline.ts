import { ref, computed, watch } from 'vue';
import { formatDate } from '@/utils/dateUtils';
import { useEventTimelineStore } from '@/stores/eventTimeline.store';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';

export function useEventTimeline(props: { familyId?: string; memberId?: string; readOnly?: boolean }) {
  const eventTimelineStore = useEventTimelineStore();

  const { list } = storeToRefs(eventTimelineStore);
  const { t } = useI18n();

  const selectedEventId = ref<string | null>(null);
  const detailDrawer = ref(false);

  const paginationLength = computed(() => {
    return Math.max(1, list.value.totalPages);
  });

  const showEventDetails = (event: Event) => {
    selectedEventId.value = event.id;
    detailDrawer.value = true;
  };

  const handleDetailClosed = () => {
    detailDrawer.value = false;
    selectedEventId.value = null;
  };

  const handlePageChange = (newPage: number) => {
    eventTimelineStore.setListOptions({
      page: newPage,
      itemsPerPage: list.value.itemsPerPage,
      sortBy: list.value.sortBy,
    });
  };

  watch(
    [() => props.familyId, () => props.memberId],
    ([newFamilyId, newMemberId]) => {
      eventTimelineStore.setFilters({ familyId: newFamilyId, memberId: newMemberId });
    },
    { immediate: true },
  );

  return {
    t,
    list,
    selectedEventId,
    detailDrawer,
    paginationLength,
    showEventDetails,
    handleDetailClosed,
    handlePageChange,
    formatDate,
  };
}
