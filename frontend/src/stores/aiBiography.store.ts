import { defineStore } from 'pinia';
import type { BiographyResultDto, AIProviderDto } from '@/types';
import { BiographyStyle, AIProviderType } from '@/types';
import i18n from '@/plugins/i18n';

export const useAIBiographyStore = defineStore('aiBiography', {
  state: () => ({
    loading: false,
    error: null as string | null,
    biographyResult: null as BiographyResultDto | null,
    lastUserPrompt: null as string | null,
    aiProviders: [] as AIProviderDto[],
    memberId: null as string | null,
    style: BiographyStyle.Emotional as BiographyStyle,
    useDBData: true,
    userPrompt: null as string | null,
    language: 'Vietnamese',
    savePromptForLater: false,
    maxTokens: 500,
    temperature: 0.7,
    selectedProvider: AIProviderType.Gemini as AIProviderType,
  }),

  actions: {
    async generateBiography() {
      if (!this.memberId) {
        this.error = i18n.global.t('aiBiography.errors.memberIdRequired');
        return;
      }

      this.loading = true;
      this.error = null;
      this.biographyResult = null;

      try {
        const result = await this.services.aiBiography.generateBiography(
          this.memberId,
          this.style,
          this.useDBData,
          this.userPrompt || undefined,
          this.language,
        );

        if (result.ok) {
          this.biographyResult = result.value;
          if (this.savePromptForLater && this.userPrompt) {
            localStorage.setItem(`lastUserPrompt_${this.memberId}`, this.userPrompt);
          }
        } else {
          this.error = result.error?.message || i18n.global.t('aiBiography.errors.generationFailed');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchLastUserPrompt(id: string) {
      this.loading = true;
      this.error = null;
      this.lastUserPrompt = null;
      try {
        const result = await this.services.aiBiography.getLastUserPrompt(id);
        if (result.ok) {
          this.lastUserPrompt = result.value || null;
        } else {
          this.error = result.error?.message || i18n.global.t('aiBiography.errors.fetchLastPromptFailed');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchAIProviders() {
      this.loading = true;
      this.error = null;
      this.aiProviders = [];
      try {
        const result = await this.services.aiBiography.getAIProviders();
        if (result.ok) {
          this.aiProviders = result.value;
        } else {
          this.error = result.error?.message || i18n.global.t('aiBiography.errors.fetchProvidersFailed');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    clearForm() {
      this.userPrompt = null;
      this.biographyResult = null;
      this.style = BiographyStyle.Emotional;
      this.useDBData = true;
      this.savePromptForLater = false;
    },

    useSavedPrompt(id: string) {
      const savedPrompt = localStorage.getItem(`lastUserPrompt_${id}`);
      if (savedPrompt) {
        this.userPrompt = savedPrompt;
      }
    },
  },
});