import { defineStore } from 'pinia';
import { useNotificationStore } from '@/stores/notification.store';
import type { GeneratedDataResponse } from '@/types';

interface NaturalLanguageInputState {
  prompt: string;
  generatedData: GeneratedDataResponse | null;
  isLoading: boolean;
  error: string | null;
}

export const useNaturalLanguageInputStore = defineStore('naturalLanguageInput', {
  state: (): NaturalLanguageInputState => ({
    prompt: '',
    generatedData: null,
    isLoading: false,
    error: null,
  }),

  actions: {
    async generateData(prompt: string) {
      this.isLoading = true;
      this.error = null;
      this.generatedData = null;
      const result = await this.services.naturalLanguageInput.generateData(prompt);

      if (result.ok) {
        this.generatedData = result.value;
      } else {
        this.error = result.error?.message || result.error?.toString() || 'Unknown error';
        useNotificationStore().showSnackbar('Error generating data: ' + this.error, 'error');
      }
      this.isLoading = false;
    },

    clearGeneratedData() {
      this.generatedData = null;
      this.prompt = '';
      this.error = null;
    },

    // This action will be called after user confirms the data
    async saveData() {
      this.isLoading = true;
      this.error = null;
      const notificationStore = useNotificationStore();

      if (!this.generatedData) {
        notificationStore.showSnackbar('No data to save.', 'error');
        this.isLoading = false;
        return;
      }

      try {
        let hasErrors = false;

        for (const family of this.generatedData.families) {
          const saveResult = await this.services.family.add(family);
          if (!saveResult.ok) {
            hasErrors = true;
            this.error = saveResult.error?.message || saveResult.error?.toString() || 'Unknown error';
            notificationStore.showSnackbar(`Error saving family ${family.name}: ` + this.error, 'error');
          }
        }

        for (const member of this.generatedData.members) {
          const saveResult = await this.services.member.add(member);
          if (!saveResult.ok) {
            hasErrors = true;
            this.error = saveResult.error?.message || saveResult.error?.toString() || 'Unknown error';
            notificationStore.showSnackbar(`Error saving member ${member.fullName}: ` + this.error, 'error');
          }
        }

        if (!hasErrors) {
          notificationStore.showSnackbar('All generated data saved successfully!', 'success');
          this.clearGeneratedData();
        } else if (!this.error) {
          // If there were errors but this.error wasn't set (e.g., first error was overwritten)
          this.error = 'Some data could not be saved.';
          notificationStore.showSnackbar(this.error, 'warning');
        }
      } catch (e: any) {
        this.error = e.message;
        notificationStore.showSnackbar('An unexpected error occurred during save.', 'error');
      } finally {
        this.isLoading = false;
      }
    },
  },
});