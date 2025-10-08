import { defineStore } from 'pinia';
import type { BiographyResultDto, AIProviderDto } from '@/types';
import { BiographyStyle, AIProviderType } from '@/types';
import i18n from '@/plugins/i18n';

export const useAIBiographyStore = defineStore('aiBiography', {
  state: () => ({
    loading: false,
    error: null as string | null,
    biographyResult: null as BiographyResultDto | null,

    aiProviders: [] as AIProviderDto[],
    memberId: null as string | null,
    style: BiographyStyle.Emotional as BiographyStyle,
    generatedFromDB: true,
    userPrompt: null as string | null,
    language: 'Vietnamese',
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
          this.generatedFromDB,
          this.userPrompt || undefined,
          this.language,
        );

        if (result.ok) {
          this.biographyResult = result.value;
        } else {
          this.error =
            result.error?.message ||
            i18n.global.t('aiBiography.errors.generationFailed');
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchLastAIBiography(memberId: string) {
      this.loading = true;
      this.error = null;
      try {
        const result =
          await this.services.aiBiography.getLastAIBiography(memberId);
        if (result.ok) {
          if (result.value) {
            this.memberId = result.value.memberId;
            this.generatedFromDB = result.value.generatedFromDB;
            this.style = result.value.style;
            this.userPrompt = result.value.userPrompt;
            this.language = result.value.language;
            this.selectedProvider = result.value.provider;
            this.biographyResult = {
              content: result.value.content,
              provider: result.value.provider,
              tokensUsed: result.value.tokensUsed,
              userPrompt: result.value.userPrompt,
              style: result.value.style,
            } as BiographyResultDto;
          }
        } else {
          this.error =
            result.error?.message ||
            i18n.global.t('aiBiography.errors.fetchLastPromptFailed');
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchAIProviders() {
      this.loading = true;
      this.error = null;
      this.aiProviders = [];
      try {
        const result = await (
          this as any
        ).services.aiBiography.getAIProviders();
        if (result.ok) {
          this.aiProviders = result.value;
          // Set selectedProvider to the first enabled provider, or default to Gemini
          if (this.aiProviders.length > 0) {
            const enabledProvider = this.aiProviders.find((p) => p.isEnabled);
            this.selectedProvider =
              enabledProvider?.providerType || AIProviderType.Gemini;
          } else {
            this.selectedProvider = AIProviderType.Gemini;
          }
        } else {
          this.error =
            result.error?.message ||
            i18n.global.t('aiBiography.errors.fetchProvidersFailed');
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    clearForm() {
      this.userPrompt = null;
      this.biographyResult = null;
      this.style = BiographyStyle.Emotional;
      this.generatedFromDB = true;
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
          this.error =
            result.error?.message ||
            i18n.global.t('aiBiography.errors.saveFailed');
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },
  },
});
