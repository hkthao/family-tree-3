import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRules } from 'vuetify/labs/rules';

interface AiAssistantFormState {
  chatInput: string;
}

export function useAiAssistantRules(state: AiAssistantFormState) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      chatInput: [
        rulesVuetify.required(),
        (value: string) => (value && value.length <= 2000) || t('aiAssistant.inputTooLong', { max: 2000 }),
      ],
    };
  });

  return rules;
}
