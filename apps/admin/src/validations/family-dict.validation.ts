import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';
import type { FamilyDictType, FamilyDictLineage, NamesByRegion } from '@/types';

interface FamilyDictFormState {
  name: string;
  type: FamilyDictType;
  description: string;
  lineage: FamilyDictLineage;
  namesByRegion: NamesByRegion;
}

export function useFamilyDictRules(_state: FamilyDictFormState) {
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      name: [rulesVuetify.required()],
      type: [rulesVuetify.required()],
      description: [rulesVuetify.required()],
      lineage: [rulesVuetify.required()],
      namesByRegion: {
        north: [rulesVuetify.required()],
        central: [rulesVuetify.required()],
        south: [rulesVuetify.required()],
      },
    };
  });

  return rules;
}
