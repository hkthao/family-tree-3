import { useI18n } from 'vue-i18n';

export function useRules() {
  const { t } = useI18n();

  const rules = {
    required: (value: any) => !!value || t('common.validations.required'),
    email: (value: string) => {
      const pattern = /^(([^<>()[\\]\\.,;:\\s@\"]+(\\.[^<>()[\\]\\.,;:\\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\\[0-9]{1,3}\\[0-9]{1,3}\\[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\\.)+[a-zA-Z]{2,}))$/;
      return pattern.test(value) || t('common.validations.email');
    },
    maxLength: (value: string, length: number) => value.length <= length || t('common.validations.maxLength', { max: length }),
    url: (value: string) => {
      const pattern = /^(https?|ftp):\/\/[^\s/$.?#].[^\s]*$/i; // More robust and correct JS regex
      return pattern.test(value) || t('common.validations.url');
    },
    positiveNumber: (value: number) => value > 0 || t('common.validations.positiveNumber'),
    // Add other common rules here
  };

  return { rules };
}