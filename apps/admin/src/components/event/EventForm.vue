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
        <v-text-field v-model="formData.code" :label="t('event.form.code')" :readonly="true" :disabled="true"
          data-testid="event-code-input"></v-text-field>
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
        <v-select v-model="formData.type" :items="eventOptionTypes" :label="t('event.form.type')"
          @blur="v$.type.$touch()" @input="v$.type.$touch()"
          :error-messages="v$.type.$errors.map((e: any) => e.$message as string)" :readonly="props.readOnly"
          data-testid="event-type-select"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select v-model="formData.calendarType" :items="calendarTypes" :label="t('event.form.calendarType')"
          @blur="v$.calendarType.$touch()" @update:modelValue="v$.calendarType.$touch()"
          :error-messages="v$.calendarType.$errors.map((e: any) => e.$message as string)" :readonly="props.readOnly"
          :disabled="props.readOnly" data-testid="event-calendar-type-select"></v-select>
      </v-col>
      <v-col cols="12" md="4">
        <v-select v-model="formData.repeatRule" :items="repeatRules" :label="t('event.form.repeatRule')"
          @blur="v$.repeatRule.$touch()" @update:modelValue="v$.repeatRule.$touch()"
          :error-messages="v$.repeatRule.$errors.map((e: any) => e.$message as string)" :readonly="props.readOnly"
          :disabled="props.readOnly" data-testid="event-repeat-rule-select"></v-select>
      </v-col>
    </v-row>

    <v-row v-if="formData.calendarType === CalendarType.Solar">
      <v-col cols="12">
        <v-date-input v-model="formData.solarDate" :label="t('event.form.solarDate')" @blur="v$.solarDate.$touch()"
          @input="v$.solarDate.$touch()" :error-messages="v$.solarDate.$errors.map((e: any) => e.$message as string)"
          :readonly="props.readOnly" prepend-icon="" dateFormat="dd/MM/yyyy"
          data-testid="event-solar-date-input" />
      </v-col>
    </v-row>

    <v-row v-if="formData.calendarType === CalendarType.Lunar">
      <template v-if="formData.lunarDate">
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
      </template>
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
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types'; // Import Event type
import { CalendarType } from '@/types/enums'; // Import enums from enums.ts
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { VColorInput } from 'vuetify/labs/VColorInput'; // Imported from vuetify/labs
import { VDateInput } from 'vuetify/labs/VDateInput'; // Imported from vuetify/labs
import { useEventForm } from '@/composables';

interface EventFormProps {
  readOnly?: boolean;
  initialEventData?: Event;
  familyId?: string;
}

const props = defineProps<EventFormProps>();

const { t } = useI18n();

const {
  formRef,
  formData,
  v$,
  eventOptionTypes,
  calendarTypes,
  repeatRules,
  lunarDays,
  lunarMonths,
  validate,
  getFormData,
} = useEventForm(props);

defineExpose({
  validate,
  getFormData,
});
</script>
