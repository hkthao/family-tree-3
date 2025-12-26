import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useRelationshipDetectorRules() {
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      familyId: [rulesVuetify.required()],
      memberAId: [rulesVuetify.required()],
      memberBId: [rulesVuetify.required()],
    };
  });

  return { rules };
}
