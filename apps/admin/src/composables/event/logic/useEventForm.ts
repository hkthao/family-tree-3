import { reactive, toRef, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventDto, AddEventDto, UpdateEventDto } from '@/types';
import type { LunarDate } from '@/types/lunar-date';
import { useEventRules, type UseEventRulesReturn } from '@/validations/event.validation';
import { cloneDeep } from 'lodash'; // Keep for now for initial formData cloning

import {
  getInitialEventFormData,
  getEventOptionTypes,
  getCalendarTypes,
  getRepeatRules,
  getLunarDays,
  getLunarMonths,
  processEventFormDataForSave,
} from './eventForm.logic';

interface EventFormProps {
  readOnly?: boolean;
  initialEventData?: EventDto;
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
  const formRef = ref<any>(null); // Ref for the v-form component

  const formData = reactive<AddEventDto | UpdateEventDto>(getInitialEventFormData(props));

  const state = reactive({
    name: toRef(formData, 'name'),
    code: toRef(formData, 'code'),
    type: toRef(formData, 'type'),
    familyId: toRef(formData, 'familyId'),
    solarDate: toRef(formData, 'solarDate'),
    calendarType: toRef(formData, 'calendarType'),
    lunarDate: reactive({
      day: toRef(formData.lunarDate as LunarDate, 'day'),
      month: toRef(formData.lunarDate as LunarDate, 'month'),
      isLeapMonth: toRef(formData.lunarDate as LunarDate, 'isLeapMonth'),
    }),
    repeatRule: toRef(formData, 'repeatRule'),
    relatedMemberIds: toRef(formData, 'relatedMemberIds'),
  });

  const eventOptionTypes = computed(() => getEventOptionTypes(t));
  const calendarTypes = computed(() => getCalendarTypes(t));
  const repeatRules = computed(() => getRepeatRules(t));

  const lunarDays = computed(() => getLunarDays());
  const lunarMonths = computed(() => getLunarMonths());

  const rules = deps.useEventRules(state);

  const validate = async () => {
    const { valid } = await formRef.value.validate();
    return valid;
  };

  const getFormData = () => {
    return processEventFormDataForSave(formData as EventDto);
  };

  return {
    formRef,
    state: {
      formData,
      rules,
      eventOptionTypes,
      calendarTypes,
      repeatRules,
      lunarDays,
      lunarMonths,
    },
    actions: {
      validate,
      getFormData,
      t, // Expose t for template usage
    },
  };
}
