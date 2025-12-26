import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useChatDrawerRules() {
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      selectedFamilyForChat: [rulesVuetify.required()],
    };
  });

  return { rules };
}
