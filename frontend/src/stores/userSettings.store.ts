import { defineStore } from 'pinia';
import { useNotificationStore } from './notification.store';
import i18n from '@/plugins/i18n'; // Import global i18n instance

export const useUserSettingsStore = defineStore('userSettings', {
  // State: Defines the reactive data of the store.
  state: () => ({
    theme: 'light' as 'light' | 'dark',
    notifications: {
      email: true,
      sms: false,
      inApp: true,
    },
    language: 'en' as 'en' | 'vi',
    loading: false,
    error: null as string | null,
  }),

  // Actions: Functions that can modify the state or perform asynchronous operations.
  actions: {
    /**
     * Sets the application theme.
     * @param theme 'light' or 'dark'
     */
    setTheme(theme: 'light' | 'dark') {
      this.theme = theme;
    },

    /**
     * Toggles a specific notification setting.
     * @param type The type of notification to toggle ('email', 'sms', or 'inApp').
     */
    toggleNotification(type: 'email' | 'sms' | 'inApp') {
      this.notifications[type] = !this.notifications[type];
    },

    /**
     * Sets the application language.
     * @param lang The language code (e.g., 'en', 'vi').
     */
    setLanguage(lang: string) {
      this.language = lang as 'en' | 'vi';
      i18n.global.locale.value = lang as 'en' | 'vi'; // Use global i18n instance
    },

    /**
     * Simulates an API call to save current user settings.
     * Returns a promise that resolves with a success message or rejects with an error.
     */
    async saveSettings(): Promise<string> {
      this.loading = true;
      this.error = null;
      const notificationStore = useNotificationStore();
      const { t } = i18n.global; // Use global i18n instance for t

      try {
        // Simulate API call delay
        await new Promise((resolve) => setTimeout(resolve, 1000));

        // Simulate success or failure
        const isSuccess = Math.random() > 0.2; // 80% chance of success

        if (isSuccess) {
          notificationStore.showSnackbar(t('userSettings.saveSuccess'), 'success');
          return t('userSettings.saveSuccess');
        } else {
          throw new Error(t('userSettings.saveError'));
        }
      } catch (err: any) {
        this.error = err.message;
        notificationStore.showSnackbar(err.message, 'error');
        throw err;
      } finally {
        this.loading = false;
      }
    },
  },
});
