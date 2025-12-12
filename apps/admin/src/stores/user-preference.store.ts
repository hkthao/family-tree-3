import { defineStore } from 'pinia';
import type { UserPreference, Theme } from '@/types';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';
import i18n from '@/plugins/i18n';

const apiUserService = new ApiUserService(apiClient);

export const useUserPreferenceStore = defineStore('userPreference', {
  state: () => ({
    loading: false,
    error: null as string | null,
    userPreference: null as UserPreference | null,
  }),

  actions: {
    async fetchUserPreferences() {
      this.loading = true;
      this.error = null;
      try {
        const result = await apiUserService.getUserPreferences();
        if (result.ok) {
          this.userPreference = result.value;
        } else {
          this.error = i18n.global.t('userSettings.preferences.fetchError');
        }
      } catch (err: any) {
        this.error = i18n.global.t('userSettings.preferences.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async setTheme(theme: Theme) {
      if (!this.userPreference) {
        // Optionally fetch preferences if not already loaded
        await this.fetchUserPreferences();
        if (!this.userPreference) {
          // If still null, create a default one or throw an error
          console.error("User preferences not loaded, cannot set theme.");
          this.error = i18n.global.t('userSettings.preferences.notLoaded');
          return;
        }
      }
      this.userPreference.theme = theme;
      // Optionally save to backend
      try {
        const result = await apiUserService.saveUserPreferences(this.userPreference);
        if (!result.ok) {
          this.error = i18n.global.t('userSettings.preferences.saveError');
        }
      } catch (err: any) {
        this.error = i18n.global.t('userSettings.preferences.unexpectedError');
      }
    },

    reset() {
      this.userPreference = null;
      this.loading = false;
      this.error = null;
    },
  },
});
