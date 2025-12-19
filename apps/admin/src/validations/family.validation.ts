import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useFamilyRules() {
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      name: [rulesVuetify.required()],
    };
  });

  return rules;
}
