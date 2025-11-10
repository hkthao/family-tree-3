import { defineStore } from 'pinia';
import type { AnalyzedDataDto } from '@/types/natural-language.d'; // Update import
import type { ApiError } from '@/plugins/axios';
import i18n from '@/plugins/i18n';
import { v4 as uuidv4 } from 'uuid'; // Import uuid for sessionId

export const useNaturalLanguageStore = defineStore('naturalLanguage', {
  state: () => ({
    input: '' as string,
    parsedData: null as AnalyzedDataDto | null, // Update type
    loading: false as boolean,
    error: null as string | null,
  }),

  actions: {
    async analyzeContent(): Promise<boolean> { // Rename action to analyzeContent
      this.loading = true;
      this.error = null;
      this.parsedData = null;

      if (!this.input.trim()) {
        this.error = i18n.global.t('naturalLanguage.errors.emptyInput');
        this.loading = false;
        return false;
      }

      const sessionId = uuidv4(); // Generate sessionId here

      const result = await this.services.naturalLanguage.analyzeContent(this.input, sessionId); // Use new service

      if (result.ok) {
        this.parsedData = result.value; // Directly assign the object
        this.loading = false;
        return true;
      } else {
        this.error =
          result.error?.message || i18n.global.t('naturalLanguage.errors.parseFailed');
        this.loading = false;
        return false;
      }
    },

    clearState() {
      this.input = '';
      this.parsedData = null;
      this.error = null;
      this.loading = false;
    },

    setInput(newInput: string) {
      this.input = newInput;
    },
  },
});
