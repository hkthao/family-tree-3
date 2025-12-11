import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n'; // Import global i18n instance
import { Theme, Language, type UserPreference } from '@/types';

export const useUserSettingsStore = defineStore('userSettings', {
  // State: Defines the reactive data of the store.
  state: () => ({
    preferences: {
      theme: Theme.Dark,
      language: Language.English,
    } as UserPreference,
  }),

  // Actions: Functions that can modify the state or perform asynchronous operations.
  actions: {
    setTheme(theme: Theme) {
      this.preferences.theme = theme;
    },

    setLanguage(language: Language) {
      this.preferences.language = language;
      i18n.global.locale.value = language === Language.English ? 'en' : 'vi';
    },
  },
});