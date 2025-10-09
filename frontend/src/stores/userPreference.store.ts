import { defineStore } from 'pinia';
import type { UserPreference } from '@/types';
import { Theme, Language } from '@/types';
import i18n from '@/plugins/i18n';

export const useUserPreferenceStore = defineStore('userPreference', {
  state: () => ({
    loading: false,
    error: null as string | null,
    preferences: {
      theme: Theme.Light,
      language: Language.English,
      emailNotificationsEnabled: true,
      smsNotificationsEnabled: false,
      inAppNotificationsEnabled: true,
    } as UserPreference,
  }),

  actions: {
    async fetchUserPreferences() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userPreference.getUserPreferences();
        if (result.ok) {
          this.preferences = result.value;
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.preferences.fetchError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.preferences.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async saveUserPreferences() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userPreference.saveUserPreferences(this.preferences);
        if (result.ok) {
          // Optionally, show a success message
          // i18n.global.t('userSettings.preferences.saveSuccess');
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.preferences.saveError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.preferences.unexpectedError');
      } finally {
        this.loading = false;
      }
    },
  },
});
