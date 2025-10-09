import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n'; // Import global i18n instance
import { Theme, Language, type UserPreference } from '@/types';

export const useUserSettingsStore = defineStore('userSettings', {
  // State: Defines the reactive data of the store.
  state: () => ({
    loading: false,
    error: null as string | null,
    preferences: {
      theme: Theme.Dark,
      language: Language.English,
      emailNotificationsEnabled: true,
      smsNotificationsEnabled: false,
      inAppNotificationsEnabled: true,
    } as UserPreference,
  }),

  // Actions: Functions that can modify the state or perform asynchronous operations.
  actions: {
    async fetchUserSettings() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userPreference.getUserPreferences();
        if (result.ok) {
          this.preferences = result.value;
          i18n.global.locale.value = this.preferences.language === Language.English ? 'en' : 'vi';
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.preferences.fetchError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.preferences.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async saveUserSettings() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userPreference.saveUserPreferences(this.preferences);
        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('userSettings.preferences.saveError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.preferences.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    setTheme(theme: Theme) {
      this.preferences.theme = theme;
    },

    setLanguage(language: Language) {
      this.preferences.language = language;
      i18n.global.locale.value = language === Language.English ? 'en' : 'vi';
    },

    toggleEmailNotifications() {
      this.preferences.emailNotificationsEnabled = !this.preferences.emailNotificationsEnabled;
    },

    toggleSmsNotifications() {
      this.preferences.smsNotificationsEnabled = !this.preferences.smsNotificationsEnabled;
    },

    toggleInAppNotifications() {
      this.preferences.inAppNotificationsEnabled = !this.preferences.inAppNotificationsEnabled;
    },
  },
});