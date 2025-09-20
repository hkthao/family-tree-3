<template>
  <v-card flat>
    <v-card-text>
      <v-toolbar flat density="compact">
        <v-toolbar-title>{{ $t('member.timeline.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn icon size="small" variant="text" @click="openAddEventForm" :disabled="readOnly">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </v-toolbar>
      <v-timeline side="end">
        <v-timeline-item
          v-for="(event, index) in timelineEvents"
          :key="index"
          :dot-color="event.color"
          size="small"
          max-width="100%"
          width="100%"
        >
          <template v-slot:opposite>
            <div
              :class="`pt-1 headline font-weight-bold text-${event.color}`"
              v-text="event.year"
            ></div>
          </template>
          <v-card>
            <v-card-title :class="`text-h6 text-${event.color}`">
              {{ event.title }}
            </v-card-title>
            <v-card-text>
              {{ event.description }}
              <v-spacer></v-spacer>
              <v-btn v-if="!readOnly" icon size="small" variant="text" @click="openEditEventForm(event)" :disabled="readOnly">
                <v-icon>mdi-pencil</v-icon>
              </v-btn>
              <v-btn v-if="!readOnly" icon size="small" variant="text" @click="$emit('delete', event)" :disabled="readOnly">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </v-card-text>
          </v-card>
        </v-timeline-item>
      </v-timeline>
    </v-card-text>

    <v-dialog v-model="eventFormDialog" max-width="600px" persistent>
      <TimelineEventForm
        :initial-event-data="selectedEvent"
        :title="isEditEventMode ? t('timeline.form.editTitle') : t('timeline.form.addTitle')"
        @submit="handleSaveTimelineEvent"
        @cancel="handleCancelTimelineEvent"
      />
    </v-dialog>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import TimelineEventForm from './TimelineEventForm.vue';

const props = defineProps<{
  timelineEvents: Array<any>; // Define a more specific type later
  readOnly?: boolean;
}>();

const emit = defineEmits(['add', 'edit', 'delete']);

const { t } = useI18n();

const eventFormDialog = ref(false);
const selectedEvent = ref<any>(null); // Define a more specific type later
const isEditEventMode = ref(false);

const openAddEventForm = () => {
  selectedEvent.value = null;
  isEditEventMode.value = false;
  eventFormDialog.value = true;
};

const openEditEventForm = (event: any) => {
  selectedEvent.value = { ...event };
  isEditEventMode.value = true;
  eventFormDialog.value = true;
};

const handleSaveTimelineEvent = (eventData: any) => {
  // This event will be handled by MemberForm.vue
  // For now, just close the dialog
  console.log('Saving timeline event:', eventData);
  eventFormDialog.value = false;
  selectedEvent.value = null;
};

const handleCancelTimelineEvent = () => {
  eventFormDialog.value = false;
  selectedEvent.value = null;
};
</script>