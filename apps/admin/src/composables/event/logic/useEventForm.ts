import { reactive, toRefs, toRef, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import type { LunarDate } from '@/types/lunar-date';
import { EventType } from '@/types';
import { CalendarType, RepeatRule } from '@/types/enums';
import { useEventRules } from '@/validations/event.validation';
import { cloneDeep } from 'lodash';

interface EventFormProps {
  readOnly?: boolean;
  initialEventData?: Event;
  familyId?: string;
}

export function useEventForm(props: EventFormProps) {
  const { t } = useI18n();
  const formRef = ref<any>(null); // Ref for the v-form component

  const formData = reactive<Omit<Event, 'id'> | Event>(
    props.initialEventData
      ? {
          ...cloneDeep(props.initialEventData),
          lunarDate: props.initialEventData.lunarDate ?? ({ day: 1, month: 1, isLeapMonth: false } as LunarDate),
        }
      : {
          name: '',
          code: '',
          type: EventType.Other,
          familyId: props.familyId || null,
          calendarType: CalendarType.Solar,
          solarDate: null,
          lunarDate: { day: 1, month: 1, isLeapMonth: false } as LunarDate,
          repeatRule: RepeatRule.None,
          description: '',
          color: '#1976D2',
          relatedMemberIds: [],
        },
  );

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

  const eventOptionTypes = computed(() => [
    { title: t('event.type.birth'), value: EventType.Birth },
    { title: t('event.type.marriage'), value: EventType.Marriage },
    { title: t('event.type.death'), value: EventType.Death },
    { title: t('event.type.other'), value: EventType.Other },
  ]);

  const calendarTypes = computed(() => [
    { title: t('event.calendarType.solar'), value: CalendarType.Solar },
    { title: t('event.calendarType.lunar'), value: CalendarType.Lunar },
  ]);

  const repeatRules = computed(() => [
    { title: t('event.repeatRule.none'), value: RepeatRule.None },
    { title: t('event.repeatRule.yearly'), value: RepeatRule.Yearly },
  ]);

  const lunarDays = computed(() => Array.from({ length: 30 }, (_, i) => i + 1));
  const lunarMonths = computed(() => Array.from({ length: 12 }, (_, i) => i + 1));

  const rules = useEventRules(state);

  const validate = async () => {
    const { valid } = await formRef.value.validate();
    return valid;
  };

  const getFormData = () => {
    const data = cloneDeep(formData);
    if (data.calendarType === CalendarType.Solar) {
      data.lunarDate = null;
    } else if (data.calendarType === CalendarType.Lunar) {
      data.solarDate = null;
    }
    return data;
  };

  return {
    formRef,
    formData,
    rules,
    eventOptionTypes,
    calendarTypes,
    repeatRules,
    lunarDays,
    lunarMonths,
    validate,
    getFormData,
  };
}
