import { defineStore } from 'pinia';
import type { UserPreference } from '@/types';
import { Theme, Language } from '@/types';
import i18n from '@/plugins/i18n';

export const useUserPreferenceStore = defineStore('userPreference', {
  state: () => ({
    loading: false,
    error: null as string | null,
    preferences: {
      id: '',
      userProfileId: '',
      theme: Theme.Dark,
      language: Language.Vietnamese,
    } as UserPreference,
  }),

  actions: {
    async fetchUserPreferences() {
              this.loading = true;
              this.error = null;
              try {
                const result = await this.services.user.getUserPreferences(); // Use services.user
                if (result.ok) {
                  this.preferences = result.value;
                } else {
                  this.error = i18n.global.t('userSettings.preferences.fetchError'); // Always use i18n key
                }
              } catch (err: any) {
                this.error = i18n.global.t('userSettings.preferences.unexpectedError'); // Always use i18n key
              } finally {
                this.loading = false;
              }
            },
        
            async saveUserPreferences() {
              this.loading = true;
              this.error = null;
              try {
                const result = await this.services.user.saveUserPreferences(this.preferences); // Use services.user
                if (result.ok) {
                  // Optionally, show a success message
                  // i18n.global.t('userSettings.preferences.saveSuccess');
                } else {
                  this.error = i18n.global.t('userSettings.preferences.saveError'); // Always use i18n key
                }
              } catch (err: any) {
                this.error = i18n.global.t('userSettings.preferences.unexpectedError'); // Always use i18n key
              } finally {
                this.loading = false;
              }
            },
          },
        });
