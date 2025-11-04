import { useI18n } from 'vue-i18n';
import { required, helpers } from '@vuelidate/validators';
import { computed, type Ref } from 'vue';
import type { Member } from '@/types';

export function useMemberRules(state: { [key: string]: Ref<any> }) {
  const { t } = useI18n();

  const dateOfDeathAfterBirth = (value: string | Date | null) => {
    if (!value) return true;
    const dateOfDeath = state.dateOfDeath.value;
    const dateOfBirth = state.dateOfBirth.value;
    if (!dateOfDeath || !dateOfBirth) return true;
    return new Date(dateOfDeath) > new Date(dateOfBirth);
  };

  const rules = computed(() => {
    return {
      lastName: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      firstName: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      dateOfBirth: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      familyId: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      dateOfDeath: { dateOfDeathAfterBirth: helpers.withMessage(() => t('validation.dateOfDeathAfterBirth'), dateOfDeathAfterBirth) },
    };
  });

  return rules;
}