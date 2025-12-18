import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';
import { useI18n } from 'vue-i18n';
import type { FamilyDictType, FamilyDictLineage, NamesByRegion } from '@/types';

interface FamilyDictFormState {
  name: string;
  type: FamilyDictType;
  description: string;
  lineage: FamilyDictLineage;
  namesByRegion: NamesByRegion;
}

export function useFamilyDictRules(_state: FamilyDictFormState) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      name: [rulesVuetify.required(t('familyDict.form.rules.nameRequired'))],
      type: [rulesVuetify.required(t('familyDict.form.rules.typeRequired'))],
      description: [rulesVuetify.required(t('familyDict.form.rules.descriptionRequired'))],
      lineage: [rulesVuetify.required(t('familyDict.form.rules.lineageRequired'))],
      namesByRegion: {
        north: [rulesVuetify.required(t('familyDict.form.rules.northRequired'))],
        central: [rulesVuetify.required(t('familyDict.form.rules.centralRequired'))],
        south: [rulesVuetify.required(t('familyDict.form.rules.southRequired'))],
      },
    };
  });

  return rules;
}
