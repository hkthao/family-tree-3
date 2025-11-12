import { useI18n } from 'vue-i18n';
import { required, email, helpers } from '@vuelidate/validators';
import { computed } from 'vue';

export function useProfileSettingsRules() {
  const { t } = useI18n();

  const rules = computed(() => {
    return {
      firstName: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      lastName: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      email: {
        required: helpers.withMessage(() => t('common.validations.required'), required),
        email: helpers.withMessage(() => t('common.validations.email'), email),
      },
    };
  });

  return rules;
}