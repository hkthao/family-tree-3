import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useFamilyMediaRules() {
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      file: [rulesVuetify.required()],
    };
  });

  return { rules };
}