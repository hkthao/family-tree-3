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
          <div v-else-if="!formattedEvents || formattedEvents.length === 0" class="d-flex justify-center align-center" style="min-height: 200px;">
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
import EventEditView from '@/views/event/EventEditView.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import EventDetailView from '@/views/event/EventDetailView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useEventCalendar } from '@/composables/event/useEventCalendar';

const props = defineProps<{
  familyId?: string;
  memberId?: string;
  readOnly?: boolean;
}>();

const emit = defineEmits(['refetchEvents']);

const {
  t,
  locale,
  canAddEvent,
  canEditEvent,
  weekdays,
  selectedDate,
  editDrawer,
  addDrawer,
  detailDrawer,
  selectedEventId,
  isDatePickerOpen,
  calendarRef,
  calendarType,
  calendarTypes,
  calendarTitle,
  prev,
  next,
  setToday,
  formattedEvents,
  getEventColor,
  showEventDetails,
  handleEventSaved,
  handleEventClosed,
  handleAddSaved,
  handleAddClosed,
  handleDetailClosed,
  handleDetailEdit,
  loading,
} = useEventCalendar(props, emit);
</script>