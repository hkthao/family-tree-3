import { reactive, toRef, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventDto, AddEventDto, UpdateEventDto } from '@/types';
import { useEventRules, type UseEventRulesReturn } from '@/validations/event.validation';
import { cloneDeep } from 'lodash';

import {
  getInitialEventFormData,
  getEventOptionTypes,
  getCalendarTypes,
  getRepeatRules,
  processEventFormDataForSave,
} from './eventForm.logic';

interface EventFormProps {
  readOnly?: boolean;
  initialEventData?: EventDto | null;
  familyId?: string;
}

interface UseEventFormDeps {
  useI18n: typeof useI18n;
  useEventRules: (state: any) => UseEventRulesReturn;
  cloneDeep: <T>(value: T) => T;
}

const defaultDeps: UseEventFormDeps = {
  useI18n,
  useEventRules,
  cloneDeep,
};

export function useEventForm(props: EventFormProps, deps: UseEventFormDeps = defaultDeps) {
  const { t } = deps.useI18n();
  const formRef = ref<any>(null);

  const formData = ref<AddEventDto | UpdateEventDto>(getInitialEventFormData(props)); // Changed to ref

  // Now, the 'state' properties will be refs to the properties of formData.value
  const state = reactive({
    name: toRef(formData.value, 'name'),
    code: toRef(formData.value, 'code'),
    type: toRef(formData.value, 'type'),
    familyId: toRef(formData.value, 'familyId'),
    solarDate: toRef(formData.value, 'solarDate'),
    calendarType: toRef(formData.value, 'calendarType'),
    lunarDate: toRef(formData.value, 'lunarDate'), // lunarDate itself is a ref to the object
    location: toRef(formData.value, 'location'),
    locationId: toRef(formData.value, 'locationId'),
    repeatRule: toRef(formData.value, 'repeatRule'),
    eventMemberIds: toRef(formData.value, 'eventMemberIds'),
  });

  const eventOptionTypes = computed(() => getEventOptionTypes(t));
  const calendarTypes = computed(() => getCalendarTypes(t));
  const repeatRules = computed(() => getRepeatRules(t));


  const rules = deps.useEventRules(state); // Pass the reactive state object

  const validate = async () => {
    const { valid } = await formRef.value.validate();
    return valid;
  };

  const getFormData = () => {
    return processEventFormDataForSave(formData.value as EventDto); // Access formData.value
  };

  return {
    formRef,
    state: {
      formData: formData.value, // Return the unwrapped value
      rules,
      eventOptionTypes,
      calendarTypes,
      repeatRules,
    },
    actions: {
      validate,
      getFormData,
      t,
    },
  };
}