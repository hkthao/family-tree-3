import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n'; // Import global i18n instance
import { Theme, Language, type UserPreference, type Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export const useUserSettingsStore = defineStore('userSettings', {
  // State: Defines the reactive data of the store.
  state: () => ({
    loading: false,
    error: null as string | null,
    preferences: {
      theme: Theme.Dark,
      language: Language.English,
    } as UserPreference,
  }),

  // Actions: Functions that can modify the state or perform asynchronous operations.
  actions: {
    async fetchUserSettings(): Promise<Result<UserPreference, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.user.getUserPreferences(); // Use services.user
      if (result.ok) {
        this.preferences = result.value;
        i18n.global.locale.value = this.preferences.language === Language.English ? 'en' : 'vi';
      } else {
        this.error = result.error?.message || i18n.global.t('userSettings.preferences.fetchError');
      }
      this.loading = false;
      return result;
    },

    async saveUserSettings(): Promise<Result<boolean, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.user.saveUserPreferences(this.preferences); // Use services.user
      if (!result.ok) {
        this.error = result.error?.message || i18n.global.t('userSettings.preferences.saveError');
      }
      this.loading = false;
      return result.ok ? { ok: true, value: true } : { ok: false, error: result.error };
    },

    setTheme(theme: Theme) {
      this.preferences.theme = theme;
    },

    setLanguage(language: Language) {
      this.preferences.language = language;
      i18n.global.locale.value = language === Language.English ? 'en' : 'vi';
    },
  },
});