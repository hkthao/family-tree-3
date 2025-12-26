import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRules } from 'vuetify/labs/rules';

interface ChatInputFormState {
  chatInput: string | null | undefined;
}

export function useChatInputRules(state: ChatInputFormState) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      chatInput: [
        (value: string | null | undefined) =>
          (value && value.length <= 1500) || t('common.validations.maxLength', { max: 1500 }),
      ],
    };
  });

  return { rules };
}
