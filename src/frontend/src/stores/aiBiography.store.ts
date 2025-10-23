import { defineStore } from 'pinia';
import type { BiographyResultDto, AIProviderDto, Member } from '@/types';
import { BiographyStyle, AIProviderType } from '@/types';
import i18n from '@/plugins/i18n';
import { useNotificationStore } from './notification.store';

export const useAIBiographyStore = defineStore('aiBiography', {
  state: () => ({
    loading: false,
    error: null as string | null,
    biographyResult: null as BiographyResultDto | null,

    aiProviders: [] as AIProviderDto[],
    memberId: null as string | null,
    currentMember: null as Member | null,
    style: BiographyStyle.Emotional as BiographyStyle,
    generatedFromDB: true,
    userPrompt: null as string | null,
    language: 'Vietnamese',
    maxTokens: 500,
    temperature: 0.7,
    selectedProvider: AIProviderType.None as AIProviderType,
  }),

  actions: {
    async fetchMemberDetails(memberId: string) {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.member.getById(memberId);
        if (result.ok) {
          this.currentMember = result.value!;
          if (this.currentMember.biography) {
            this.biographyResult = { content: this.currentMember.biography };
          }
        } else {
          this.error =
            result.error?.message ||
            i18n.global.t('aiBiography.errors.fetchMemberFailed');
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('aiBiography.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

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
        const result = await this.services.member.updateMemberBiography(
          memberId,
          content,
        ); // Changed

        if (result.ok) {
          console.log('Biography saved successfully:', result.value);
          // Optionally, update the currentMember's biography in the store
          if (this.currentMember) {
            this.currentMember.biography = content;
          }
          const notificationStore = useNotificationStore();
          notificationStore.showSnackbar(
            i18n.global.t('aiBiography.success.save'),
            'success',
          );
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
