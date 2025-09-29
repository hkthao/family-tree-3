<template>
  <v-form class="mt-3" ref="form" :disabled="props.readOnly">
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="eventForm.name"
          :label="t('event.form.name')"
          :rules="[rules.required]"
          :readonly="props.readOnly"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          v-model="eventForm.type"
          :items="eventTypes"
          :label="t('event.form.type')"
          :rules="[rules.required]"
          :readonly="props.readOnly"
        ></v-select>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <FamilyAutocomplete
          v-model="computedFamilyId"
          :label="t('event.form.family')"
          :rules="[rules.required]"
          :read-only="props.readOnly"
          :multiple="false"
        />
      </v-col>
      <v-col cols="12" md="6">
        <MemberAutocomplete
          v-model="eventForm.relatedMembers"
          :label="t('event.form.relatedMembers')"
          :read-only="props.readOnly"
          clearable
          multiple
        />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <DateInputField
          v-model="eventForm.startDate"
          :label="t('event.form.startDate')"
          :rules="[rules.required]"
          :readonly="props.readOnly"
        />
      </v-col>
      <v-col cols="12" md="6">
        <DateInputField
          v-model="eventForm.endDate"
          :label="t('event.form.endDate')"
          optional
          :readonly="props.readOnly"
        />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field
          v-model="eventForm.location"
          :label="t('event.form.location')"
          :readonly="props.readOnly"
        ></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-textarea
          v-model="eventForm.description"
          :label="t('event.form.description')"
          :readonly="props.readOnly"
        ></v-textarea>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-color-picker
          v-model="eventForm.color"
          :label="t('event.form.color')"
          :readonly="props.readOnly"
          hide-canvas
          hide-sliders
          show-swatches
        ></v-color-picker>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types/event/event';
import { EventType } from '@/types/event/event-type'; // Import EventType enum
import DateInputField from '@/components/common/DateInputField.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';

const props = defineProps<{
  readOnly?: boolean;
  initialEventData?: Event;
}>();

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);

const eventForm = ref<Omit<Event, 'id'> | Event>(
  props.initialEventData || {
    name: '',
    type: EventType.Other, // Use enum value
    familyId: null,
    startDate: null,
    endDate: null,
    location: '',
    description: '',
    color: '#1976D2',
    relatedMembers: [],
  },
);

const eventTypes = [
  { title: t('event.type.birth'), value: EventType.Birth }, // Use enum value
  { title: t('event.type.marriage'), value: EventType.Marriage }, // Use enum value
  { title: t('event.type.death'), value: EventType.Death }, // Use enum value
  { title: t('event.type.migration'), value: EventType.Migration }, // Use enum value
  { title: t('event.type.other'), value: EventType.Other }, // Use enum value
];

const rules = {
  required: (value: unknown) => !!value || t('validation.required'),
};

onMounted(async () => {});

const computedFamilyId = computed<string | undefined>({
  get: () => eventForm.value.familyId ?? undefined,
  set: (value) => {
    eventForm.value.familyId = value ?? null;
  },
});

// Expose form validation and data for parent component
const validate = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    return valid;
  }
  return false;
};

const getFormData = () => {
  return eventForm.value;
};

defineExpose({
  validate,
  getFormData,
});
</script>
