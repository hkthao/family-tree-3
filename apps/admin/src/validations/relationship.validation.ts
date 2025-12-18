import { useI18n } from 'vue-i18n';
import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useRelationshipRules(state: { sourceMemberId: any; targetMemberId: any }) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const notSameAs = (value: any) => {
    return value !== state.targetMemberId || t('relationship.validation.notSame');
  };

  const rules = computed(() => {
    return {
      sourceMemberId: [
        rulesVuetify.required(),
        notSameAs,
      ],
      targetMemberId: [rulesVuetify.required()],
      type: [rulesVuetify.required()],
      familyId: [rulesVuetify.required()],
    };
  });

  return rules;
}