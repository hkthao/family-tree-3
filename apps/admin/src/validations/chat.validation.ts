import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useChatInputRules() {
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      chatInput: [
        rulesVuetify.maxLength(1500),
      ],
    };
  });

  return { rules };
}
