import { useI18n } from 'vue-i18n';
import { required, helpers } from '@vuelidate/validators';
import { computed } from 'vue';
import type { Ref } from 'vue';

export function useEventRules(state: { [key: string]: Ref<any> }) {
  const { t } = useI18n();

  const endDateAfterStartDate = (value: string | null) => {
    if (!value || !state.startDate.value) return true;
    const endDate = new Date(value);
    const startDate = new Date(state.startDate.value);
    return endDate >= startDate;
  };

  const rules = computed(() => {
    return {
      name: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      type: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      familyId: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      startDate: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      endDate: { endDateAfterStartDate: helpers.withMessage(() => t('event.validation.endDateAfterStartDate'), endDateAfterStartDate) },
    };
  });

  return rules;
}
