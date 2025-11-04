import { useI18n } from 'vue-i18n';
import { required, helpers } from '@vuelidate/validators';
import { computed } from 'vue';
import type { Relationship } from '@/types';

export function useRelationshipRules(state: Partial<Relationship>) {
  const { t } = useI18n();

  const notSameAs = (value: any) => {
    return value !== state.targetMemberId;
  };

  const rules = computed(() => {
    return {
      sourceMemberId: {
        required: helpers.withMessage(() => t('common.validations.required'), required),
        notSameAs: helpers.withMessage(() => t('relationship.validation.notSame'), notSameAs),
      },
      targetMemberId: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      type: { required: helpers.withMessage(() => t('common.validations.required'), required) },
      familyId: { required: helpers.withMessage(() => t('common.validations.required'), required) },
    };
  });

  return rules;
}