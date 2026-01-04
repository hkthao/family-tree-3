<template>
  <div>
    <div v-if="isLoading" class="text-center py-4">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p>{{ t('common.loading') }}</p>
    </div>
    <v-alert v-else-if="error" type="error" class="mb-4">{{ error?.message || t('event.errors.load') }}</v-alert>
    <v-list v-else-if="memberEvents && memberEvents.length > 0">
      <v-list-item v-for="event in memberEvents" :key="event.id" class="mb-2" :title="event.name"
        :subtitle="getEventDate(event)">
        <template v-slot:prepend>
          <v-avatar color="primary" class="text-white">
            <v-icon>mdi-calendar-range</v-icon>
          </v-avatar>
        </template>
        <template v-slot:append>
          <v-btn icon variant="text" size="small" :to="{ name: 'event-detail', params: { id: event.id } }"
            v-if="event.id">
            <v-icon>mdi-information</v-icon>
            <v-tooltip activator="parent" location="top">{{ t('event.detail.title') }}</v-tooltip>
          </v-btn>
        </template>
      </v-list-item>
    </v-list>
    <v-alert v-else type="info" variant="tonal" class="ma-2">{{ t('event.list.noEvents') }}</v-alert>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useMemberEventsQuery } from '@/composables/event/useMemberEventsQuery';
import type { EventDto } from '@/types';
import { format } from 'date-fns';
import { CalendarType } from '@/types/enums'; // Import CalendarType enum

interface MemberEventsTabProps {
  memberId: string;
}

const props = defineProps<MemberEventsTabProps>();
const { t } = useI18n();

const { data: memberEvents, isLoading, error } = useMemberEventsQuery(props.memberId);

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