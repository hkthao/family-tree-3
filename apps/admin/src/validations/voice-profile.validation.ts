import { useI18n } from 'vue-i18n';
import type { FamilyMedia } from '@/types';

export function useVoiceProfileValidation() {
  const { t } = useI18n();

  const voiceProfileRules = {
    label: [(value: string) => !!value || t('common.validations.required')],
    rawAudioUrls: [
      (value: FamilyMedia[]) => (value && value.length > 0) || t('common.validations.required'),
    ],
    durationSeconds: [
      (value: number) => !!value || t('common.validations.required'),
      (value: number) => value > 0 || t('common.validations.positiveNumber'),
    ],
    language: [(value: string) => !!value || t('common.validations.required')],
    consent: [(value: boolean) => !!value || t('common.validations.consentRequired')],
  };

  return { voiceProfileRules };
}
