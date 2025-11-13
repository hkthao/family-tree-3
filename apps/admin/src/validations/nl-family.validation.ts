import { useI18n } from 'vue-i18n';
import { required, maxLength, helpers } from '@vuelidate/validators';
import { computed } from 'vue';

export function useNLFamilyRules() {
  const { t } = useI18n();

  const rules = computed(() => {
    return {
      prompt: {
        required: helpers.withMessage(() => t('common.validations.required'), required),
        maxLength: helpers.withMessage(({
          $params
        }) => t('common.validations.maxLength', { max: $params.max }), maxLength(1000)),
      },
    };
  });

  return rules;
}