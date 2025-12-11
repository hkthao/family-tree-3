<template>
  <div>
    <v-toolbar>
      <v-btn variant="text" @click="setToday">{{ t('common.today') }}</v-btn>
      <v-btn variant="text" size="small" icon @click="prev">
        <v-icon>mdi-chevron-left</v-icon>
      </v-btn>
      <v-menu v-model="isDatePickerOpen" :close-on-content-click="false">
        <template v-slot:activator="{ props }">
          <v-toolbar-title v-bind="props" class="text-center cursor-pointer">{{ calendarTitle }}</v-toolbar-title>
        </template>
        <v-date-picker v-model="selectedDate" @update:model-value="isDatePickerOpen = false"></v-date-picker>
      </v-menu>
      <v-btn variant="text" size="small" icon @click="next">
        <v-icon>mdi-chevron-right</v-icon>
      </v-btn>
      <v-spacer></v-spacer>
      <v-select :width="50" v-model="calendarType" :items="calendarTypes" class="me-2"
        :label="t('event.calendar.viewMode')" hide-details :readonly="props.readOnly"></v-select>
      <v-btn color="primary" icon @click="addDrawer = true" data-testid="add-new-event-button" v-if="canAddEvent">
        <v-tooltip :text="t('event.list.action.create')">
          <template v-slot:activator="{ props }">
            <v-icon v-bind="props" icon="mdi-plus" />
          </template>
        </v-tooltip>
      </v-btn>
    </v-toolbar>
        <div>
          <div v-if="loading" class="d-flex justify-center align-center" style="min-height: 200px;">
            <v-progress-circular indeterminate color="primary"></v-progress-circular>
          </div>
          <div v-else-if="!events || events.length === 0" class="d-flex justify-center align-center" style="min-height: 200px;">
            <v-alert type="info" dense>{{ t('event.calendar.noEvents') }}</v-alert>
          </div>
          <v-calendar v-else class="mt-2" ref="calendarRef" v-model="selectedDate" :events="formattedEvents"
            :event-color="getEventColor" :type="calendarType" event-overlap-mode="stack" :locale="locale" :key="locale"
            :weekdays="weekdays">
            <template #event="{ event }">
              <div class="v-event-summary" @click="showEventDetails(event.eventObject)">
                {{ event.title }}
              </div>
              <div class="v-event-description">
                {{ event.eventObject.description }}
              </div>
            </template>
          </v-calendar>
        </div>

    <BaseCrudDrawer v-model="editDrawer" v-if="canEditEvent" @close="handleEventClosed">
      <EventEditView v-if="selectedEventId && editDrawer" :event-id="selectedEventId" @close="handleEventClosed"
        @saved="handleEventSaved" />
    </BaseCrudDrawer>
    <BaseCrudDrawer v-model="addDrawer" v-if="canAddEvent" @close="handleAddClosed">
      <EventAddView v-if="addDrawer" :family-id="props.familyId" @close="handleAddClosed" @saved="handleAddSaved" />
    </BaseCrudDrawer>

    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <EventDetailView v-if="detailDrawer && selectedEventId" :event-id="selectedEventId" @close="handleDetailClosed"
        @edit="handleDetailEdit" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import EventEditView from '@/views/event/EventEditView.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import EventDetailView from '@/views/event/EventDetailView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useAuth } from '@/composables';
import { useUpcomingEvents } from '@/composables/data/useUpcomingEvents'; // Import useUpcomingEvents

type CalendarEventColorFunction = (event: { [key: string]: any }) => string;

const props = defineProps<{
  familyId?: string;
  memberId?: string;
  readOnly?: boolean;
}>();

const emit = defineEmits(['refetchEvents']);

const { t, locale } = useI18n();
const { isAdmin, isFamilyManager } = useAuth();

const { upcomingEvents: events, isLoading: loading, refetch: refetchEvents } = useUpcomingEvents(toRef(props, 'familyId'));

const canAddEvent = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const canEditEvent = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const weekdays = computed(() => [0, 1, 2, 3, 4, 5, 6]);

const selectedDate = ref(new Date());
const editDrawer = ref(false);
const addDrawer = ref(false);
const detailDrawer = ref(false);
const selectedEventId = ref<string | null>(null);
const isDatePickerOpen = ref(false);

const calendarRef = ref<{
  title: string;
  prev: () => void;
  next: () => void;
  value: Date;
} | null>(null);
const calendarType = ref<
  | 'month'
  | 'week'
  | 'day'
  | '4day'
  | 'category'
  | 'custom-daily'
  | 'custom-weekly'
>('month');
const calendarTypes = computed(() => [
  { title: t('event.calendar.viewMode.month'), value: 'month' },
  { title: t('event.calendar.viewMode.week'), value: 'week' },
  { title: t('event.calendar.viewMode.day'), value: 'day' },
  { title: t('event.calendar.viewMode.4day'), value: '4day' },
]);

const calendarTitle = computed(() => {
  if (calendarRef.value) {
    // This will ideally update when calendarRef updates
    // For now, it will show static or initial value
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
  if (!events.value) return [];
  return events.value
    .filter((event) => event.startDate)
    .map((event) => ({
      title: event.name,
      start: new Date(event.startDate as Date),
      end: event.endDate
        ? new Date(event.endDate)
        : new Date(event.startDate as Date),
      color: event.color || 'primary',
      timed: true,
      eventObject: event,
    }));
});

const getEventColor: CalendarEventColorFunction = (event: {
  [key: string]: any;
}) => {
  return event.color;
};

const showEventDetails = (eventSlotScope: Event) => {
  selectedEventId.value = eventSlotScope.id;
  if (canEditEvent.value) {
    editDrawer.value = true;
  } else {
    detailDrawer.value = true;
  }
};

const handleEventSaved = () => {
  editDrawer.value = false;
  selectedEventId.value = null;
  refetchEvents(); // Refetch data
};

const handleEventClosed = () => {
  editDrawer.value = false;
  selectedEventId.value = null;
};

const handleAddSaved = () => {
  addDrawer.value = false;
  refetchEvents(); // Refetch data
};

const handleAddClosed = () => {
  addDrawer.value = false;
};

const handleDetailClosed = () => {
  detailDrawer.value = false;
  selectedEventId.value = null;
};

const handleDetailEdit = (event: Event) => {
  detailDrawer.value = false;
  selectedEventId.value = event.id;
  editDrawer.value = true;
};

// Watch for changes in selectedDate for calendar navigation, but not for data fetching
watch(
  selectedDate,
  (newDate) => {
    // Logic here for calendar view changes if needed, but not data fetching
  },
  { immediate: true },
);
</script>