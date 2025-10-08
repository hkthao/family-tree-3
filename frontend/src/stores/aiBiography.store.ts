import { defineStore } from 'pinia';
import type { BiographyResultDto, AIProviderDto, AIBiography } from '@/types';
import { BiographyStyle, AIProviderType } from '@/types';
import i18n from '@/plugins/i18n';

export const useAIBiographyStore = defineStore('aiBiography', {
  state: () => ({
    loading: false,
    error: null as string | null,
    biographyResult: null as BiographyResultDto | null,
    lastAIBiography: null as AIBiography | null,

    aiProviders: [] as AIProviderDto[],
    memberId: null as string | null,
    style: BiographyStyle.Emotional as BiographyStyle,
    useDBData: true,
    userPrompt: null as string | null,
    language: 'Vietnamese',
    savePromptForLater: false,
    maxTokens: 500,
    temperature: 0.7,
    selectedProvider: AIProviderType.None as AIProviderType,
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
        } else {
          this.error = result.error?.message || i18n.global.t('aiBiography.errors.generationFailed');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchLastAIBiography(id: string) {
      this.loading = true;
      this.error = null;
      this.lastAIBiography = null;
      try {
        const result = await this.services.aiBiography.getLastAIBiography(id);
        if (result.ok) {
          this.lastAIBiography = result.value || null;
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
        const result = await (this as any).services.aiBiography.getAIProviders();
        if (result.ok) {
          this.aiProviders = result.value;
          // Set selectedProvider to the first enabled provider, or default to Gemini
          if (this.aiProviders.length > 0) {
            const enabledProvider = this.aiProviders.find(p => p.isEnabled);
            this.selectedProvider = enabledProvider?.providerType || AIProviderType.Gemini;
          } else {
            this.selectedProvider = AIProviderType.Gemini;
          }
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

    useLastUsedPrompt() {
      if (this.lastAIBiography?.userPrompt) {
        this.userPrompt = this.lastAIBiography.userPrompt;
      }
    },
    async saveBiography(memberId: string, content: string) {
      if (!memberId || !content) {
        this.error = i18n.global.t('aiBiography.errors.saveFailed');
        return;
      }

      this.loading = true;
      this.error = null;

      try {
        const result = await (this as any).services.aiBiography.saveBiography({
          memberId: memberId,
          style: this.style,
          content: content,
          provider: this.biographyResult?.provider || AIProviderType.Gemini,
          userPrompt: this.userPrompt || '',
          generatedFromDB: this.biographyResult?.generatedFromDB || false,
          tokensUsed: this.biographyResult?.tokensUsed || 0,
        });

        if (result.ok) {
          console.log('Biography saved successfully:', result.value);
        } else {
          this.error = result.error?.message || i18n.global.t('aiBiography.errors.saveFailed');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },
  },
});