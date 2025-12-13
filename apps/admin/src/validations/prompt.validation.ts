// apps/admin/src/validations/prompt.validation.ts

import { useI18n } from 'vue-i18n';
import { required, helpers, maxLength } from '@vuelidate/validators';
import { computed, type Ref } from 'vue';

export function usePromptRules(_state: { [key: string]: Ref<any> }) {
  const { t } = useI18n();

  const rules = computed(() => {
    return {
      code: {
        required: helpers.withMessage(() => t('prompt.form.rules.codeRequired'), required),
        maxLength: helpers.withMessage(() => t('prompt.form.rules.codeMaxLength', { max: 50 }), maxLength(50)),
      },
      title: {
        required: helpers.withMessage(() => t('prompt.form.rules.titleRequired'), required),
        maxLength: helpers.withMessage(() => t('prompt.form.rules.titleMaxLength', { max: 100 }), maxLength(100)),
      },
      content: {
        required: helpers.withMessage(() => t('prompt.form.rules.contentRequired'), required),
      },
      description: {
        maxLength: helpers.withMessage(() => t('prompt.form.rules.descriptionMaxLength', { max: 500 }), maxLength(500)),
      },
    };
  });

  return rules;
}
