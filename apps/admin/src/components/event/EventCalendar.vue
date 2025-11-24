<template>
  <div>
  <v-toolbar>
    <v-btn variant="text" @click="setToday">{{ t('common.today') }}</v-btn>
    <v-btn variant="text" size="small" icon @click="prev">
      <v-icon>mdi-chevron-left</v-icon>
    </v-btn>
    <v-toolbar-title class="text-center">{{ calendarTitle }}</v-toolbar-title>
    <v-btn variant="text" size="small" icon @click="next">
      <v-icon>mdi-chevron-right</v-icon>
    </v-btn>
    <v-spacer></v-spacer>
    <v-select :width="50" v-model="calendarType" :items="calendarTypes" class="me-2"
      :label="t('event.calendar.viewMode')" hide-details :readonly="props.readOnly"></v-select>
    <v-btn color="primary" icon @click="addDrawer = true" data-testid="add-new-event-button" v-if="canAddEvent">
      <v-tooltip :text="t('event.list.action.create')">
        <template v-slot:activator="{ props }">
          <v-icon v-bind="props">mdi-plus</v-icon>
        </template>
      </v-tooltip>
    </v-btn>
  </v-toolbar>
  <v-calendar class="mt-2" ref="calendarRef" v-model="selectedDate" :events="formattedEvents"
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

          <v-navigation-drawer v-model="editDrawer" location="right" temporary width="650" v-if="canEditEvent">
            <EventEditView v-if="selectedEventId && editDrawer" :event-id="selectedEventId" @close="handleEventClosed"
              @saved="handleEventSaved" />
          </v-navigation-drawer>  <v-navigation-drawer v-model="addDrawer" location="right" temporary width="650" v-if="canAddEvent">
    <EventAddView v-if="addDrawer" :family-id="props.familyId" @close="handleAddClosed" @saved="handleAddSaved" />
  </v-navigation-drawer>

  <v-navigation-drawer v-model="detailDrawer" location="right" temporary width="650">
    <EventDetailView v-if="detailDrawer && selectedEventId" :event-id="selectedEventId" @close="handleDetailClosed"
      @edit="handleDetailEdit" />
  </v-navigation-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import { useEventStore } from '@/stores/event.store'; // Import event store
import EventEditView from '@/views/event/EventEditView.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import EventDetailView from '@/views/event/EventDetailView.vue'; // Import EventDetailView
import { useAuth } from '@/composables/useAuth';

// Define CalendarEventColorFunction type to match v-calendar's expectation
type CalendarEventColorFunction = (event: { [key: string]: any }) => string;

const props = defineProps<{
  familyId?: string; // Optional prop for family ID
  memberId?: string; // Optional prop for member ID
  readOnly?: boolean; // Add readOnly prop
}>();

const { t, locale } = useI18n();
const eventStore = useEventStore(); // Initialize event store
const { isAdmin, isFamilyManager } = useAuth();

const canAddEvent = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const canEditEvent = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const weekdays = computed(() => [0, 1, 2, 3, 4, 5, 6]); // Sunday to Saturday

const selectedDate = ref(new Date());
const editDrawer = ref(false); // Control visibility of the edit drawer
const addDrawer = ref(false); // Control visibility of the add drawer
const detailDrawer = ref(false); // Control visibility of the detail drawer
const selectedEventId = ref<string | null>(null); // Store the ID of the event being edited
// const editableEvent = ref<Event | undefined>(undefined); // No longer needed

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

const loadEvents = async () => {
  if (!props.familyId && !props.memberId) {
    eventStore.list.items = [];
    eventStore.list.totalItems = 0;
    return;
  }

  eventStore.setPage(1); // Always fetch all for calendar view
  eventStore.setItemsPerPage(-1); // Fetch all for calendar view

  const filters: any = {};
  if (props.memberId) {
    filters.relatedMemberId = props.memberId;
  } else if (props.familyId) {
    filters.familyId = props.familyId;
  }
  eventStore.list.filter = filters;
  await eventStore._loadItems();
};

const formattedEvents = computed(() => {
  const events = eventStore.list.items
    .filter((event) => event.startDate) // Only include events with a valid startDate
    .map((event) => ({
      title: event.name,
      start: new Date(event.startDate as Date),
      end: event.endDate
        ? new Date(event.endDate)
        : new Date(event.startDate as Date),
      color: event.color || 'primary',
      timed: true,
      eventObject: event, // Store the original event object
    }));
  return events;
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
  loadEvents(); // Reload events after saving
};

const handleEventClosed = () => {
  editDrawer.value = false;
  selectedEventId.value = null;
};

const handleAddSaved = () => {
  addDrawer.value = false;
  loadEvents(); // Reload events after adding
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

watch(
  [() => props.familyId, () => props.memberId],
  () => {
    loadEvents();
  },
  { immediate: true },
);

onMounted(() => {
  loadEvents();
});
</script>
