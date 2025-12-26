import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRules } from 'vuetify/labs/rules';

export function useRelationshipDetectorRules() {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      familyId: [(v: string | undefined | null) => !!v || t('relationshipDetection.familyRequired')],
      memberAId: [(v: string | undefined | null) => !!v || t('relationshipDetection.memberARequired')],
      memberBId: [(v: string | undefined | null) => !!v || t('relationshipDetection.memberBRequired')],
    };
  });

  return { rules };
}
