import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useProfileSettingsRules() {
  const rulesVuetify = useRules()
  const rules = computed(() => {
    return {
      firstName: [rulesVuetify.required()],
      lastName: [rulesVuetify.required()],
      email: [
        rulesVuetify.required(),
        rulesVuetify.email(),
      ],
    };
  });

  return rules;
}