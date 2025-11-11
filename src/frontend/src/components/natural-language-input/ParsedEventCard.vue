<template>
  <v-card class="border d-flex flex-column" :elevation="0" fill-height width="100%">
    <v-progress-linear :active="event.loading" :indeterminate="event.loading" color="primary" absolute
      top></v-progress-linear>
    <v-card-item>
      <v-card-title class="text-h6 text-center">
        <v-icon icon="mdi-calendar-text" class="mr-2"></v-icon> {{ serialNumber }}. {{ title }}
      </v-card-title>
    </v-card-item>

    <v-card-text class="py-0">
      <v-chip-group column>
        <v-chip v-for="detail in details" size="small" :key="detail.originalKey">
          <strong>{{ detail.label }}:</strong> {{ detail.value }}
        </v-chip>
      </v-chip-group>

      <div v-if="recommendations.length > 0">
        <v-chip v-for="(rec, index) in recommendations" :key="`rec-${index}`" color="warning" size="small">
          {{ rec }}
        </v-chip>
      </div>

      <div class="my-2">
        <v-alert v-if="event.errorMessage" type="error" class="m-0">
          {{ event.errorMessage }}
        </v-alert>
        <v-alert v-if="event.saveAlert?.show" :type="event.saveAlert?.type" class="m-0" variant="tonal">
          {{ event.saveAlert?.message }}
        </v-alert>
      </div>
    </v-card-text>
    <v-spacer></v-spacer>
    <v-card-actions v-if="!event.savedSuccessfully">
      <v-spacer></v-spacer>
      <v-btn color="red" @click="deleteEvent" size="small">{{ t('common.delete') }}</v-btn>
      <v-btn color="primary" @click="emit('save-event', event)" :disabled="!!event.errorMessage || event.loading"
        :loading="event.loading" size="small">{{ t('common.save')
        }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventDataDto } from '@/types/natural-language.d';
import { EventType } from '@/types';

const props = defineProps<{
  event: EventDataDto;
  serialNumber: number;
}>();

const emit = defineEmits(['delete', 'save-event']);

const { t } = useI18n();

const eventTypeToString = (type: EventType): string => {
  switch (type) {
    case EventType.Birth:
      return 'birth';
    case EventType.Death:
      return 'death';
    case EventType.Marriage:
      return 'marriage';
    case EventType.Other:
    default:
      return 'other';
  }
};

const title = computed(() => {
  const eventTypeString = eventTypeToString(props.event.type);
  return t(`event.type.${eventTypeString}`);
});

const formatDate = (dateString: string | null | undefined) => {
  if (!dateString) return '';
  try {
    const date = new Date(dateString);
    if (!isNaN(date.getTime())) {
      const day = String(date.getDate()).padStart(2, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const year = date.getFullYear();
      return `${year}-${month}-${day}`;
    }
  } catch (e) {
    // Fallback to original string if parsing fails
  }
  return dateString;
};

const details = computed(() => {
  const detailsArray: { label: string; value: any; originalKey: string }[] = [];

  detailsArray.push({ label: t('event.form.description'), value: props.event.description, originalKey: 'description' });
  if (props.event.date) detailsArray.push({ label: t('event.form.date'), value: formatDate(props.event.date), originalKey: 'eventDate' });
  if (props.event.location) detailsArray.push({ label: t('event.form.location'), value: props.event.location, originalKey: 'location' });
  // Related members are handled in the NLEditorView, not displayed directly on the card for simplicity
  return detailsArray;
});

const recommendations = computed(() => {
  const recs: string[] = [];
  if (props.event.errorMessage) {
    recs.push(props.event.errorMessage);
  }

  if (!props.event.date) recs.push(t('naturalLanguage.recommendations.missingDate'));
  if (!props.event.location) recs.push(t('naturalLanguage.recommendations.missingLocation'));
  return recs;
});

const deleteEvent = () => {
  emit('delete');
};
</script>
