import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRules } from 'vuetify/labs/rules';
import type { FamilyLinkRequestDto } from '@/types';

export function useFamilyLinkRequestRules(formData: Partial<FamilyLinkRequestDto>) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const targetFamilyCannotBeRequestingFamily = (value: string | null | undefined) =>
    value !== formData.requestingFamilyId || t('familyLinkRequest.form.rules.targetCannotBeRequesting');

  const rules = computed(() => {
    return {
      requestingFamilyId: [rulesVuetify.required()],
      targetFamilyId: [rulesVuetify.required(), targetFamilyCannotBeRequestingFamily],
      requestMessage: [rulesVuetify.required()],
    };
  });

  return { rules };
}