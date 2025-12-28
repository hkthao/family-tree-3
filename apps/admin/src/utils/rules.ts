// apps/admin/src/utils/rules.ts
import i18n from '@/plugins/i18n';

export const rules = {
  required: (value: any) => !!value || i18n.global.t('common.validations.required'),
  positiveNumber: (value: number) => value > 0 || i18n.global.t('common.validations.positiveNumber'),
  // Add other common rules here
};
