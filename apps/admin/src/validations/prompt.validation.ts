import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function usePromptRules() {
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      code: [
        rulesVuetify.required(),
        rulesVuetify.maxLength(50),
      ],
      title: [
        rulesVuetify.required(),
        rulesVuetify.maxLength(100),
      ],
      content: [rulesVuetify.required()],
      description: [rulesVuetify.maxLength(500)],
    };
  });

  return rules;
}
