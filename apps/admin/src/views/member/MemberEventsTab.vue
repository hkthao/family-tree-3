<template>
  <div>
    <div v-if="isLoading" class="text-center py-4">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p>{{ t('common.loading') }}</p>
    </div>
    <v-alert v-else-if="error" type="error" class="mb-4">{{ error?.message || t('event.errors.load') }}</v-alert>
    <v-list v-else-if="memberEvents && memberEvents.length > 0">
      {{ console.log('MemberEventsTab - memberEvents:', memberEvents) }}
      <v-list-item v-for="event in memberEvents" :key="event.id" class="mb-2" :title="event.name"
        :subtitle="getEventDate(event)">
        <template v-slot:prepend>
          <v-avatar color="primary" class="text-white">
            <v-icon>mdi-calendar-range</v-icon>
          </v-avatar>
        </template>
        <template v-slot:append>
          <v-btn icon variant="text" size="small" @click="showEventDetails(event.id)" v-if="event.id">
            <v-icon>mdi-information</v-icon>
            <v-tooltip activator="parent" location="top">{{ t('event.detail.title') }}</v-tooltip>
          </v-btn>
        </template>
      </v-list-item>
    </v-list>
    <v-alert v-else type="info" variant="tonal" class="ma-2">{{ t('event.list.noEvents') }}</v-alert>

    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <EventDetailView v-if="detailDrawer && selectedEventId" :event-id="selectedEventId" @close="handleDetailClosed" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'; // Import ref
import { useI18n } from 'vue-i18n';
import { useMemberEventsQuery } from '@/composables/event/useMemberEventsQuery';
import type { EventDto } from '@/types';
import { format } from 'date-fns';
import { CalendarType } from '@/types/enums'; // Import CalendarType enum
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // Import BaseCrudDrawer
import EventDetailView from '@/views/event/EventDetailView.vue'; // Import EventDetailView

interface MemberEventsTabProps {
  memberId: string;
}

const props = defineProps<MemberEventsTabProps>();
const { t } = useI18n();

const { data: memberEvents, isLoading, error } = useMemberEventsQuery(props.memberId);

const detailDrawer = ref(false);
const selectedEventId = ref<string | null>(null);

const showEventDetails = (eventId: string) => {
  selectedEventId.value = eventId;
  detailDrawer.value = true;
};

const handleDetailClosed = () => {
  detailDrawer.value = false;
  selectedEventId.value = null;
};

const getEventDate = (event: EventDto): string => {
  if (event.calendarType === CalendarType.Solar && event.solarDate) {
    return format(new Date(event.solarDate), 'dd/MM/yyyy');
  }
  if (event.calendarType === CalendarType.Lunar && event.lunarDate) {
    return `${event.lunarDate.day}/${event.lunarDate.month} (Âm lịch)`;
  }
  return t('common.unknown');
};
</script>

<style scoped></style>