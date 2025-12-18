import { useRules } from 'vuetify/labs/rules';
import { useI18n } from 'vue-i18n';
import type { FamilyLocation } from '@/types';

export const useFamilyLocationValidationRules = () => {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = {
    name: [
      rulesVuetify.required(t('common.validations.required')),
      rulesVuetify.minLength(3, t('common.validations.minLength', { min: 3 })),
      rulesVuetify.maxLength(100, t('common.validations.maxLength', { max: 100 })),
    ],
    locationType: [
      rulesVuetify.required(t('common.validations.required')),
    ],
    accuracy: [
      rulesVuetify.required(t('common.validations.required')),
    ],
    source: [
      rulesVuetify.required(t('common.validations.required')),
    ],
    latitude: [], // Remove custom numeric validation
    longitude: [], // Remove custom numeric validation
  };

  return rules;
};
