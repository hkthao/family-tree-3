import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRules } from 'vuetify/labs/rules';
import type { FamilyLinkRequestDto } from '@/types';

export function useFamilyLinkRequestRules(formData: Partial<FamilyLinkRequestDto>) {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const required = (propertyType: string) => (value: string | null | undefined) =>
    !!value || t(`familyLinkRequest.form.rules.${propertyType}Required`);

  const targetFamilyCannotBeRequestingFamily = (value: string | null | undefined) =>
    value !== formData.requestingFamilyId || t('familyLinkRequest.form.rules.targetCannotBeRequesting');

  const rules = computed(() => {
    return {
      requestingFamilyId: [required('requestingFamilyId')],
      targetFamilyId: [required('targetFamilyId'), targetFamilyCannotBeRequestingFamily],
      requestMessage: [required('requestMessage')],
    };
  });

  return { rules };
}