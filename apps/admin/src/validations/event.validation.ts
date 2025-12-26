import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';
import { CalendarType, type RepeatRule } from '@/types/enums';
import { EventType } from '@/types';
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
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      name: [rulesVuetify.required()],
      code: [],
      type: [rulesVuetify.required()],
      familyId: [rulesVuetify.required()],
      calendarType: [rulesVuetify.required()],
      repeatRule: [rulesVuetify.required()],
      solarDate: [rulesVuetify.required()],
      lunarDate: {
        day: [rulesVuetify.required()],
        month: [rulesVuetify.required()],
        isLeapMonth: [],
      },
    };
  });

  return rules;
}

export type UseEventRulesReturn = ReturnType<typeof useEventRules>;