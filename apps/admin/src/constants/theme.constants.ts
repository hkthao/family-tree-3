import { Theme } from '@/types';
import type { ComposerTranslation } from 'vue-i18n';

export const getThemeOptions = (t: ComposerTranslation) => [
  {
    text: t('userSettings.preferences.themeLight'),
    value: Theme.Light,
    code: 'light'
  },
  {
    text: t('userSettings.preferences.themeDark'),
    value: Theme.Dark,
    code: 'dark'
  },
];
