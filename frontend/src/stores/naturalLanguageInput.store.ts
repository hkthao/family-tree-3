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
        let saveResult;
        if (this.generatedData.dataType === 'Family' && this.generatedData.family) {
          saveResult = await this.services.family.add(this.generatedData.family);
        } else if (this.generatedData.dataType === 'Member' && this.generatedData.member) {
          saveResult = await this.services.member.add(this.generatedData.member);
        } else {
          notificationStore.showSnackbar('Unknown data type or missing data, cannot save.', 'error');
          this.isLoading = false;
          return;
        }

        if (saveResult.ok) {
          notificationStore.showSnackbar(`${this.generatedData.dataType} data saved successfully!`, 'success');
          this.clearGeneratedData();
        } else {
          this.error = saveResult.error?.message || saveResult.error?.toString() || 'Unknown error';
          notificationStore.showSnackbar(`Error saving ${this.generatedData.dataType} data: ` + this.error, 'error');
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