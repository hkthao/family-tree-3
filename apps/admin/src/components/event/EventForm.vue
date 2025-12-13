<template>
  <v-form class="mt-3" ref="formRef" :disabled="props.readOnly">
    <v-row>
      <v-col cols="12">
        <family-auto-complete v-model="formData.familyId" :label="t('event.form.family')" @blur="v$.familyId.$touch()"
          @update:modelValue="v$.familyId.$touch()" :error-messages="v$.familyId.$errors.map(e => e.$message as string)"
          :read-only="props.readOnly" :multiple="false" :disabled="true" data-testid="event-family-autocomplete" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.name" :label="t('event.form.name')" @blur="v$.name.$touch()"
          @input="v$.name.$touch()" :error-messages="v$.name.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="event-name-input"></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="4">
        <v-select v-model="formData.type" :items="eventTypes" :label="t('event.form.type')" @blur="v$.type.$touch()"
          @input="v$.type.$touch()" :error-messages="v$.type.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" data-testid="event-type-select"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-date-input v-model="formData.startDate" :label="t('event.form.startDate')" @blur="v$.startDate.$touch()"
          @input="v$.startDate.$touch()" :error-messages="v$.startDate.$errors.map(e => e.$message as string)"
          :readonly="props.readOnly" prepend-icon="" append-inner-icon="mdi-calendar" format="dd/MM/yyyy"
          data-testid="event-start-date-input" />
      </v-col>
      <v-col cols="12" md="4">
        <v-date-input v-model="formData.endDate" :label="t('event.form.endDate')" optional :readonly="props.readOnly"
          @blur="v$.endDate.$touch()" @input="v$.endDate.$touch()"
          :error-messages="v$.endDate.$errors.map(e => e.$message as string)" data-testid="event-end-date-input"
          prepend-icon="" append-inner-icon="mdi-calendar" format="dd/MM/yyyy" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.location" :label="t('event.form.location')" :readonly="props.readOnly"
          data-testid="event-location-input"></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :label="t('event.form.description')" :readonly="props.readOnly"
          data-testid="event-description-input"></v-textarea>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <MemberAutocomplete
          v-model="formData.relatedMemberIds"
          :label="t('event.form.relatedMembers')"
          :family-id="formData.familyId || undefined"
          :disabled="props.readOnly"
          :multiple="true"
          data-testid="event-related-members-autocomplete"
        />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-color-input v-model="formData.color" :label="t('event.form.color')" :disabled="props.readOnly"
          data-testid="event-color-picker" pip-location="append-inner">
        </v-color-input>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { reactive, toRefs, ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import { EventType } from '@/types';
import { useVuelidate } from '@vuelidate/core';
import { useEventRules } from '@/validations/event.validation';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';

interface EventFormProps {
  readOnly?: boolean;
  initialEventData?: Event;
  familyId?: string;
}

const props = defineProps<EventFormProps>();

const { t } = useI18n();

const formRef = ref<HTMLFormElement | null>(null);

const formData = reactive<Omit<Event, 'id'> | Event>(
  props.initialEventData || {
    name: '',
    type: EventType.Other,
    familyId: props.familyId || null, // Use prop familyId if provided
    startDate: null,
    endDate: null,
    location: '',
    description: '',
    color: '#1976D2',
    relatedMemberIds: [],
  },
);

const state = reactive({
  name: toRef(formData, 'name'),
  type: toRef(formData, 'type'),
  familyId: toRef(formData, 'familyId'), // Added familyId to state
  startDate: toRef(formData, 'startDate'),
  endDate: toRef(formData, 'endDate'),
  relatedMemberIds: toRef(formData, 'relatedMemberIds'),
});

const eventTypes = [
  { title: t('event.type.birth'), value: EventType.Birth },
  { title: t('event.type.marriage'), value: EventType.Marriage },
  { title: t('event.type.death'), value: EventType.Death },
  { title: t('event.type.migration'), value: EventType.Migration },
  { title: t('event.type.other'), value: EventType.Other },
];

const rules = useEventRules(toRefs(state));

const v$ = useVuelidate(rules, state);

// Expose form validation and data for parent component
const validate = async () => {
  return await v$.value.$validate();
};

const getFormData = () => {
  return formData;
};


defineExpose({
  validate,
  getFormData,
});
</script>
