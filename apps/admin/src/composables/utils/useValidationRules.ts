import { useI18n } from 'vue-i18n';

export const useValidationRules = () => {
  const { t } = useI18n();

  const rules = {
    required: (value: any) => !!value || t('validation.required'),
    email: (value: string) => {
      const pattern = /^(([^<>()[\\]\\.,;:\s@\"]+(\.[^<>()[\\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
      return pattern.test(value) || t('validation.email');
    },
    min: (length: number) => (value: string) =>
      (value && value.length >= length) || t('validation.min', { length }),
    max: (length: number) => (value: string) =>
      (value && value.length <= length) || t('validation.max', { length }),
    url: (value: string) => { // New URL validation rule
      const pattern = /^(https?:\/\/|ftp:\/\/)[^\s/$.?#].[^\s]*$/i;
      return pattern.test(value) || t('validation.url');
    },
  };

  return {
    rules,
  };
};
