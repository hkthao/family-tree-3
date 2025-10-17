import { Language } from '@/types';
import type { ComposerTranslation } from 'vue-i18n';

export const getLanguageOptions = (t: ComposerTranslation) => [
  {
    text: t('userSettings.preferences.languageEnglish'),
    value: Language.English,
    code: 'en',
  },
  {
    text: t('userSettings.preferences.languageVietnamese'),
    value: Language.Vietnamese,
    code: 'vi',
  },
];
