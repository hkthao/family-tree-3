import { useI18n } from 'vue-i18n';
import { required, helpers, between, requiredIf } from '@vuelidate/validators';
import { computed } from 'vue';
import type { Ref } from 'vue';
import { CalendarType } from '@/types/enums';

export function useEventRules(state: { [key: string]: Ref<any> }) {
  const { t } = useI18n();

  const rules = computed(() => {
    return {
      name: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      code: {}, // Mã không bắt buộc
      type: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      familyId: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      calendarType: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      repeatRule: { required: helpers.withMessage(() => t('common.validations.required'), required) },

      solarDate: {
        required: helpers.withMessage(
          () => t('common.validations.required'),
          requiredIf(() => state.calendarType.value === CalendarType.Solar),
        ),
      },

      lunarDate: {
        day: {
          required: helpers.withMessage(
            () => t('common.validations.required'),
            requiredIf(() => state.calendarType.value === CalendarType.Lunar),
          ),
          between: helpers.withMessage(() => t('event.validation.lunarDayBetween'), between(1, 30)),
        },
        month: {
          required: helpers.withMessage(
            () => t('common.validations.required'),
            requiredIf(() => state.calendarType.value === CalendarType.Lunar),
          ),
          between: helpers.withMessage(() => t('event.validation.lunarMonthBetween'), between(1, 12)),
        },
        isLeapMonth: {}, // No specific validation for this boolean
      },
    };
  });

  return rules;
}