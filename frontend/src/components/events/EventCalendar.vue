<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('event.calendar.title') }}
    </v-card-title>
    <v-card-text>
      <v-toolbar flat>
        <v-btn class="ma-2" @click="setToday">{{ t('common.today') }}</v-btn>
        <v-btn icon class="ma-2" @click="prev">
          <v-icon>mdi-chevron-left</v-icon>
        </v-btn>
        <v-toolbar-title class="text-center">{{ calendarTitle }}</v-toolbar-title>
        <v-btn icon class="ma-2" @click="next">
          <v-icon>mdi-chevron-right</v-icon>
        </v-btn>
        <v-spacer></v-spacer>
        <v-select
          v-model="calendarType"
          :items="calendarTypes"
          class="ma-2"
          density="comfortable"
          :label="t('event.calendar.viewMode')"
          variant="outlined"
          hide-details
        ></v-select>
      </v-toolbar>
      <v-sheet>
        <v-calendar
          ref="calendarRef"
          v-model="selectedDate"
          :events="formattedEvents"
          :event-color="getEventColor"
          :type="calendarType"
          event-overlap-mode="stack"
          :locale="locale"
          :key="locale"
          :weekdays="weekdays"
        >
          <template #event="{ event }">
            <div class="v-event-summary" @click="showEventDetails(event.eventObject)">
              {{ event.title }}
            </div>
            <div class="v-event-description">
              {{ event.eventObject.description }}
            </div>
          </template>
        </v-calendar>
      </v-sheet>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyEvent } from '@/types/family-event';

interface CalendarEvent {
  color: string;
  title: string;
  start: Date;
  end: Date;
  timed: boolean;
  eventObject: FamilyEvent;
}

// Define CalendarEventColorFunction type to match v-calendar's expectation
type CalendarEventColorFunction = (event: { [key: string]: any }) => string;

// Define EventSlotScope type to match v-calendar's event-click slot
interface EventSlotScope {
  event: CalendarEvent;
  nativeEvent: MouseEvent;
  // Add other properties if needed based on v-calendar's documentation
}

const props = defineProps<{
  events: FamilyEvent[];
}>();

const { t, locale } = useI18n();

const weekdays = computed(() => [0, 1, 2, 3, 4, 5, 6]); // Sunday to Saturday

const selectedDate = ref(new Date());
// Removed invalid import of VCalendar

const calendarRef = ref<{ title: string; prev: () => void; next: () => void; value: Date } | null>(null);
const calendarType = ref<'month' | 'week' | 'day' | '4day' | 'category' | 'custom-daily' | 'custom-weekly'>('month');
const calendarTypes = computed(() => [
  { title: t('event.calendar.viewMode.month'), value: 'month' },
  { title: t('event.calendar.viewMode.week'), value: 'week' },
  { title: t('event.calendar.viewMode.day'), value: 'day' },
  { title: t('event.calendar.viewMode.4day'), value: '4day' },
]);

const calendarTitle = computed(() => {
  if (calendarRef.value) {
    return calendarRef.value.title;
  }
  return '';
});

const prev = () => {
  if (calendarRef.value) {
    calendarRef.value.prev();
  }
};

const next = () => {
  if (calendarRef.value) {
    calendarRef.value.next();
  }
};

const setToday = () => {
  if (calendarRef.value) {
    calendarRef.value.value = new Date();
  }
};

const formattedEvents = computed(() => {
  const events = props.events
    .filter(event => event.startDate) // Only include events with a valid startDate
    .map(event => ({
      title: event.name,
      start: new Date(event.startDate as Date),
      end: event.endDate ? new Date(event.endDate) : new Date(event.startDate as Date),
      color: event.color || 'primary',
      timed: true,
      eventObject: event, // Store the original event object
    }));
  return events;
});


const getEventColor: CalendarEventColorFunction = (event: { [key: string]: any }) => {
  return event.color;
};

const emit = defineEmits(['viewEvent']);

const showEventDetails = (eventSlotScope: CalendarEvent ) => {
  emit('viewEvent', eventSlotScope.eventObject);
};
</script>
