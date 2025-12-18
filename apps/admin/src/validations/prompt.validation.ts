import { useI18n } from 'vue-i18n';
import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function usePromptRules() {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      code: [
        rulesVuetify.required(t('prompt.form.rules.codeRequired')),
        rulesVuetify.maxLength(50, t('prompt.form.rules.codeMaxLength', { max: 50 })),
      ],
      title: [
        rulesVuetify.required(t('prompt.form.rules.titleRequired')),
        rulesVuetify.maxLength(100, t('prompt.form.rules.titleMaxLength', { max: 100 })),
      ],
      content: [rulesVuetify.required(t('prompt.form.rules.contentRequired'))],
      description: [rulesVuetify.maxLength(500, t('prompt.form.rules.descriptionMaxLength', { max: 500 }))],
    };
  });

  return rules;
}
