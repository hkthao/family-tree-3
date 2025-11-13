import { useI18n } from 'vue-i18n';
import { required, helpers } from '@vuelidate/validators';
import { computed } from 'vue';
import type { Ref } from 'vue';

export function useRelationshipRules(state: { [key: string]: Ref<any> }) {
  const { t } = useI18n();

  const notSameAs = (value: any) => {
    return value !== state.targetMemberId.value;
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