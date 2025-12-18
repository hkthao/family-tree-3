import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';
import { useI18n } from 'vue-i18n';
import { CalendarType, type RepeatRule } from '@/types/enums';
import type { EventType } from '@/types';
import type { LunarDate } from '@/types/lunar-date';

interface EventFormState {
  name: string;
  code: string;
  type: EventType;
  familyId: string | null | undefined;
  solarDate: Date | null | undefined;
  calendarType: CalendarType;
  lunarDate: LunarDate;
  repeatRule: RepeatRule;
  relatedMemberIds: string[] | undefined;
}

export function useEventRules(_state: EventFormState) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      name: [rulesVuetify.required(t('event.validation.nameRequired'))],
      code: [],
      type: [rulesVuetify.required(t('event.validation.typeRequired'))],
      familyId: [rulesVuetify.required(t('event.validation.familyIdRequired'))],
      calendarType: [rulesVuetify.required(t('event.validation.calendarTypeRequired'))],
      repeatRule: [rulesVuetify.required(t('event.validation.repeatRuleRequired'))],
      solarDate: [rulesVuetify.required(t('event.validation.solarDateRequired'))],
      lunarDate: {
        day: [rulesVuetify.required(t('event.validation.lunarDayRequired'))],
        month: [rulesVuetify.required(t('event.validation.lunarMonthRequired'))],
        isLeapMonth: [],
      },
    };
  });

  return rules;
}