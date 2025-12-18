import { useI18n } from 'vue-i18n';
import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useMemberRules(state: { dateOfBirth: any; dateOfDeath: any }) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const dateOfDeathAfterBirth = (value: string | Date | null) => {
    if (!value) return true;
    const dateOfDeath = state.dateOfDeath;
    const dateOfBirth = state.dateOfBirth;
    if (!dateOfDeath || !dateOfBirth) return true;
    return new Date(dateOfDeath) > new Date(dateOfBirth) || t('validation.dateOfDeathAfterBirth');
  };

  const isPositive = (value: number | null | undefined) => {
    if (value === null || value === undefined) {
      return true; // Optional field
    }
    return value > 0 || t('common.validations.positiveNumber');
  };

  const rules = computed(() => {
    return {
      lastName: [rulesVuetify.required(t('common.validations.required'))],
      firstName: [rulesVuetify.required(t('common.validations.required'))],
      dateOfBirth: [],
      familyId: [rulesVuetify.required(t('common.validations.required'))],
      dateOfDeath: [dateOfDeathAfterBirth],
      order: [isPositive],
    };
  });

  return rules;
}