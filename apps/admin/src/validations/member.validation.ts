import { useI18n } from 'vue-i18n';
import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';
import type { MemberAddDto, MemberUpdateDto } from '@/types';

export function useMemberRules(formData: MemberAddDto | MemberUpdateDto) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const dateOfDeathAfterBirth = (value: string | Date | null) => {
    if (!value) return true;
    const dateOfDeath = formData.dateOfDeath;
    const dateOfBirth = formData.dateOfBirth;
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
      lastName: [rulesVuetify.required()],
      firstName: [rulesVuetify.required()],
      dateOfBirth: [], // Now optional

      familyId: [rulesVuetify.required()],
      dateOfDeath: [dateOfDeathAfterBirth],
      lunarDateOfDeath: [], // Now optional and treated as number
      order: [isPositive],
    };
  });

  return rules;
}