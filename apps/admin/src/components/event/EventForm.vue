<template>
  <v-form class="mt-3" ref="formRef" :disabled="props.readOnly">
    <v-row>
      <v-col cols="12">
        <family-auto-complete v-model="formData.familyId" :label="actions.t('event.form.family')"
          :rules="rules.familyId"
          :read-only="props.readOnly" :multiple="false" :disabled="true" data-testid="event-family-autocomplete"
          prepend-inner-icon="mdi-home-heart" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.code" :label="actions.t('event.form.code')" :readonly="true" :disabled="true"
          data-testid="event-code-input" prepend-inner-icon="mdi-identifier"></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.name" :label="actions.t('event.form.name')"
          :rules="rules.name"
          :readonly="props.readOnly" data-testid="event-name-input" prepend-inner-icon="mdi-format-title"></v-text-field>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="4">
        <v-select v-model="formData.type" :items="eventOptionTypes" :label="actions.t('event.form.type')"
          :rules="rules.type"
          :readonly="props.readOnly"
          data-testid="event-type-select" prepend-inner-icon="mdi-tag"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select v-model="formData.calendarType" :items="calendarTypes" :label="actions.t('event.form.calendarType')"
          :rules="rules.calendarType"
          :readonly="props.readOnly"
          :disabled="props.readOnly" data-testid="event-calendar-type-select" prepend-inner-icon="mdi-calendar-month"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select v-model="formData.repeatRule" :items="repeatRules" :label="actions.t('event.form.repeatRule')"
          :rules="rules.repeatRule"
          :readonly="props.readOnly"
          :disabled="props.readOnly" data-testid="event-repeat-rule-select" prepend-inner-icon="mdi-repeat"></v-select>
      </v-col>
    </v-row>

    <v-row v-if="formData.calendarType === CalendarType.Solar">
      <v-col cols="12">
        <v-date-input v-model="formData.solarDate" :label="actions.t('event.form.solarDate')"
          :rules="rules.solarDate"
          :readonly="props.readOnly" prepend-icon="" dateFormat="dd/MM/yyyy"
          data-testid="event-solar-date-input" prepend-inner-icon="mdi-calendar" />
      </v-col>
    </v-row>

    <v-row v-if="formData.calendarType === CalendarType.Lunar">
      <LunarDateInput
        v-model="formData.lunarDate!"
        :read-only="props.readOnly"
        :label-day="actions.t('event.form.lunarDay')"
        :label-month="actions.t('event.form.lunarMonth')"
        :label-is-leap-month="actions.t('event.form.isLeapMonth')"
      />
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :label="actions.t('event.form.description')" :readonly="props.readOnly"
          data-testid="event-description-input" prepend-inner-icon="mdi-text-box-outline"></v-textarea>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <LocationInputField
          v-model:model-value="formData.location"
          v-model:location-id="formData.locationId"
          :label="actions.t('event.form.location')"
          :family-id="formData.familyId || undefined"
          :read-only="props.readOnly"
          prepend-inner-icon="mdi-map-marker"
        ></LocationInputField>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <MemberAutocomplete v-model="formData.eventMemberIds" :label="actions.t('event.form.relatedMembers')"
          :family-id="formData.familyId || undefined" :disabled="props.readOnly" :multiple="true"
          data-testid="event-related-members-autocomplete" prepend-inner-icon="mdi-account-group" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-color-input v-model="formData.color" :label="actions.t('event.form.color')" :disabled="props.readOnly"
          data-testid="event-color-picker" pip-location="append-inner" prepend-inner-icon="mdi-palette">
        </v-color-input>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import type { EventDto } from '@/types'; // Import Event type
import { CalendarType } from '@/types/enums'; // Import enums from enums.ts
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import LocationInputField from '@/components/common/LocationInputField.vue'; // NEW import
import LunarDateInput from '@/components/common/LunarDateInput.vue';
import { VColorInput } from 'vuetify/labs/VColorInput'; // Imported from vuetify/labs
import { VDateInput } from 'vuetify/labs/VDateInput'; // Imported from vuetify/labs
import { useEventForm } from '@/composables';

interface EventFormProps {
  readOnly?: boolean;
  initialEventData?: EventDto | null; // Allow null for initial data
  familyId?: string;
}

const props = defineProps<EventFormProps>();

const {
  formRef,
  state,
  actions,
} = useEventForm(props);

const {
  formData,
  rules,
  eventOptionTypes,
  calendarTypes,
  repeatRules,
} = state;

const {
  validate,
  getFormData,
} = actions;

defineExpose({
  validate,
  getFormData,
});
</script>
