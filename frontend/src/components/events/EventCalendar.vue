<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('event.calendar.title') }}
    </v-card-title>
    <v-card-text>
      <v-sheet>
        <v-calendar
          ref="calendar"
          v-model="selectedDate"
          :events="formattedEvents"
          :event-color="getEventColor"
          type="month"
          event-overlap-mode="stack"
          @click:event="showEventDetails"
          :locale="locale.value"
          :key="locale.value"
          :weekdays="weekdays"
        ></v-calendar>
      </v-sheet>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types/event';

const props = defineProps<{
  events: Event[];
}>();

const { t, locale } = useI18n();

const weekdays = computed(() => [0, 1, 2, 3, 4, 5, 6]); // Sunday to Saturday

const selectedDate = ref(new Date());

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

const getEventColor = (event: any) => {
  return event.color;
};

const showEventDetails = (event: any) => {
  console.log('Raw event object clicked:', event);
  // Emit an event or use a global store to show event details in a dialog
  console.log('Event clicked:', event.eventObject);
  // For now, just logging. In a real app, you'd open a dialog with event.event.eventObject
};
</script>

<style scoped>
/* Add any specific styles for the calendar here */
</style>
