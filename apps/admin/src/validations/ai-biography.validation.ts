import { useI18n } from 'vue-i18n';
import { maxLength, helpers } from '@vuelidate/validators';
import { computed } from 'vue';

export function useAIBiographyRules() {
  const { t } = useI18n();

  const rules = computed(() => {
    return {
      userPrompt: {
        maxLength: helpers.withMessage(({
          $params
        }) => t('common.validations.maxLength', { max: $params.max }), maxLength(1000)),
      },
    };
  });

  return rules;
}