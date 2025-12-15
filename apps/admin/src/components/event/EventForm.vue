<template>
  <v-form class="mt-3" ref="formRef" :disabled="props.readOnly">
    <v-row>
      <v-col cols="12">
        <family-auto-complete v-model="formData.familyId" :label="t('event.form.family')" @blur="v$.familyId.$touch()"
          @update:modelValue="v$.familyId.$touch()" :error-messages="v$.familyId.$errors.map((e: any) => e.$message as string)"
          :read-only="props.readOnly" :multiple="false" :disabled="true" data-testid="event-family-autocomplete" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.name" :label="t('event.form.name')" @blur="v$.name.$touch()"
          @input="v$.name.$touch()" :error-messages="v$.name.$errors.map((e: any) => e.$message as string)"
          :readonly="props.readOnly" data-testid="event-name-input"></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="4">
        <v-select v-model="formData.type" :items="eventTypes" :label="t('event.form.type')" @blur="v$.type.$touch()"
          @input="v$.type.$touch()" :error-messages="v$.type.$errors.map((e: any) => e.$message as string)"
          :readonly="props.readOnly" data-testid="event-type-select"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select v-model="formData.calendarType" :items="calendarTypes" :label="t('event.form.calendarType')"
          @blur="v$.calendarType.$touch()" @update:modelValue="v$.calendarType.$touch()"
          :error-messages="v$.calendarType.$errors.map((e: any) => e.$message as string)" :readonly="props.readOnly"
          data-testid="event-calendar-type-select"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select v-model="formData.repeatRule" :items="repeatRules" :label="t('event.form.repeatRule')"
          @blur="v$.repeatRule.$touch()" @update:modelValue="v$.repeatRule.$touch()"
          :error-messages="v$.repeatRule.$errors.map((e: any) => e.$message as string)" :readonly="props.readOnly"
          data-testid="event-repeat-rule-select"></v-select>
      </v-col>
    </v-row>

    <v-row v-if="formData.calendarType === CalendarType.Solar">
      <v-col cols="12">
        <v-date-input v-model="formData.solarDate" :label="t('event.form.solarDate')" @blur="v$.solarDate.$touch()"
          @input="v$.solarDate.$touch()" :error-messages="v$.solarDate.$errors.map((e: any) => e.$message as string)"
          :readonly="props.readOnly" prepend-icon="" format="dd/MM/yyyy"
          data-testid="event-solar-date-input" />
      </v-col>
    </v-row>

    <v-row v-if="formData.calendarType === CalendarType.Lunar && formData.lunarDate">
      <v-col cols="12" md="4">
        <v-select v-model.number="formData.lunarDate.day" :items="lunarDays" :label="t('event.form.lunarDay')"
          @blur="v$.lunarDate.day.$touch()" @update:modelValue="v$.lunarDate.day.$touch()"
          :error-messages="v$.lunarDate.day.$errors.map((e: any) => e.$message as string)" :readonly="props.readOnly"
          data-testid="event-lunar-day-input"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select v-model.number="formData.lunarDate.month" :items="lunarMonths" :label="t('event.form.lunarMonth')"
          @blur="v$.lunarDate.month.$touch()" @update:modelValue="v$.lunarDate.month.$touch()"
          :error-messages="v$.lunarDate.month.$errors.map((e: any) => e.$message as string)" :readonly="props.readOnly"
          data-testid="event-lunar-month-input"></v-select>
      </v-col>
      <v-col cols="12" md="4" class="d-flex align-center">
        <v-checkbox v-model="formData.lunarDate.isLeapMonth" :label="t('event.form.isLeapMonth')"
          :readonly="props.readOnly" data-testid="event-lunar-is-leap-month-input"></v-checkbox>
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
        <MemberAutocomplete v-model="formData.relatedMemberIds" :label="t('event.form.relatedMembers')"
          :family-id="formData.familyId || undefined" :disabled="props.readOnly" :multiple="true"
          data-testid="event-related-members-autocomplete" />
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
import { reactive, toRefs, toRef, computed } from 'vue'; // Import computed
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types'; // Import Event type
import type { LunarDate } from '@/types/lunar-date'; // Import LunarDate type from its specific file
import { EventType } from '@/types'; // EventType is still from '@/types'
import { CalendarType, RepeatRule } from '@/types/enums'; // Import enums from enums.ts
import { useVuelidate } from '@vuelidate/core';
import { useEventRules } from '@/validations/event.validation';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { cloneDeep } from 'lodash';
import VDateInput from '@/components/common/VDateInput.vue'; // Assuming you have a custom date input
import VColorInput from '@/components/common/VColorInput.vue'; // Assuming you have a custom color input

interface EventFormProps {
  readOnly?: boolean;
  initialEventData?: Event;
  familyId?: string;
}

const props = defineProps<EventFormProps>();

const { t } = useI18n();

const formData = reactive<Omit<Event, 'id'> | Event>(
  props.initialEventData
    ? cloneDeep(props.initialEventData)
    : {
        name: '',
        type: EventType.Other,
        familyId: props.familyId || null,
        calendarType: CalendarType.Solar, // Default to Solar
        solarDate: null,
        lunarDate: { day: 0, month: 0, isLeapMonth: false } as LunarDate, // Initialize lunarDate with 0
        repeatRule: RepeatRule.None, // Default to None
        description: '',
        color: '#1976D2',
        relatedMemberIds: [],
      },
);

// Adjust Vuelidate state to match rules structure for lunarDate
const state = reactive({
  name: toRef(formData, 'name'),
  type: toRef(formData, 'type'),
  familyId: toRef(formData, 'familyId'),
  calendarType: toRef(formData, 'calendarType'),
  solarDate: toRef(formData, 'solarDate'),
  // lunarDate needs to be an object for Vuelidate with day/month properties
  lunarDate: reactive({
    day: toRef(formData.lunarDate!, 'day'),
    month: toRef(formData.lunarDate!, 'month'),
    isLeapMonth: toRef(formData.lunarDate!, 'isLeapMonth'),
  }),
  repeatRule: toRef(formData, 'repeatRule'),
  relatedMemberIds: toRef(formData, 'relatedMemberIds'),
});

const eventTypes = [
  { title: t('event.type.birth'), value: EventType.Birth },
  { title: t('event.type.marriage'), value: EventType.Marriage },
  { title: t('event.type.death'), value: EventType.Death },
  { title: t('event.type.other'), value: EventType.Other }, // Removed Migration
];

const calendarTypes = [
  { title: t('event.calendarType.solar'), value: CalendarType.Solar },
  { title: t('event.calendarType.lunar'), value: CalendarType.Lunar },
];

const repeatRules = [
  { title: t('event.repeatRule.none'), value: RepeatRule.None },
  { title: t('event.repeatRule.yearly'), value: RepeatRule.Yearly },
];

// Computed properties for lunar day and month select options
const lunarDays = computed(() => Array.from({ length: 30 }, (_, i) => i + 1));
const lunarMonths = computed(() => Array.from({ length: 12 }, (_, i) => i + 1));

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
