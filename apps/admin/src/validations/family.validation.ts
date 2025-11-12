import { useI18n } from 'vue-i18n';
import { required, helpers } from '@vuelidate/validators';
import { computed } from 'vue';

export function useFamilyRules() {
  const { t } = useI18n();

  const rules = computed(() => {
    return {
      name: { required: helpers.withMessage(() => t('common.validations.required'), required) },
    };
  });

  return rules;
}
