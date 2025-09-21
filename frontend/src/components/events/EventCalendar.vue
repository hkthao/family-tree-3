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
        ></v-calendar>
      </v-sheet>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types/event';
import { VCalendar } from 'vuetify/labs/VCalendar';

const props = defineProps<{
  events: Event[];
}>();

const { t } = useI18n();

const selectedDate = ref(new Date());

const formattedEvents = computed(() => {
  return props.events.map(event => ({
    title: event.name,
    start: event.startDate ? new Date(event.startDate) : new Date(),
    end: event.endDate ? new Date(event.endDate) : event.startDate ? new Date(event.startDate) : new Date(),
    color: event.color || 'primary',
    timed: true,
    eventObject: event, // Store the original event object
  }));
});

const getEventColor = (event: any) => {
  return event.color;
};

const showEventDetails = (event: { nativeEvent: Event; event: { eventObject: Event; }; }) => {
  // Emit an event or use a global store to show event details in a dialog
  console.log('Event clicked:', event.event.eventObject);
  // For now, just logging. In a real app, you'd open a dialog with event.event.eventObject
};
</script>

<style scoped>
/* Add any specific styles for the calendar here */
</style>
