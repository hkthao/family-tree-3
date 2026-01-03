import { useI18n } from 'vue-i18n';

export function useVoiceProfileValidation() {
  const { t } = useI18n();

  const voiceProfileRules = {
    label: [(value: string) => !!value || t('common.validations.required')],
    memberId: [(value: string) => !!value || t('common.validations.required')],
    rawAudioUrls: [
      (value: string[]) => (value && value.length > 0) || t('common.validations.required'),
    ],

    language: [(value: string) => !!value || t('common.validations.required')],
    consent: [(value: boolean) => !!value || t('common.validations.consentRequired')],
  };

  return { voiceProfileRules };
}
