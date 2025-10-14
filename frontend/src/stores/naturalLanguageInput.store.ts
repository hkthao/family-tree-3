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
    async saveData(data: any, dataType: string) {
      this.isLoading = true;
      this.error = null;
      const notificationStore = useNotificationStore();

      try {
        // let saveResult;
        if (dataType === 'Family') {
          // Assuming a family service exists with a create method
          // saveResult = await this.services.family.add(data);
          notificationStore.showSnackbar('Family data saved successfully (mock)!', 'success');
          console.log('Saving Family data:', data);
        } else if (dataType === 'Member') {
          // Assuming a member service exists with a create method
          // saveResult = await this.services.member.add(data);
          notificationStore.showSnackbar('Member data saved successfully (mock)!', 'success');
          console.log('Saving Member data:', data);
        } else {
          notificationStore.showSnackbar('Unknown data type, cannot save.', 'error');
          this.isLoading = false;
          return;
        }

        // In a real implementation, check saveResult.ok
        // if (saveResult.ok) {
        //   notificationStore.showSnackbar(`${dataType} data saved successfully!`, 'success');
        //   this.clearGeneratedData();
        // } else {
        //   this.error = saveResult.error;
        //   notificationStore.showSnackbar(`Error saving ${dataType} data: ` + saveResult.error, 'error');
        // }
        this.clearGeneratedData(); // Clear after mock save
      } catch (e: any) {
        this.error = e.message;
        notificationStore.showSnackbar('An unexpected error occurred during save.', 'error');
      } finally {
        this.isLoading = false;
      }
    },
  },
});