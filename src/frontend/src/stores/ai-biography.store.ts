import { defineStore } from 'pinia';
import type { BiographyResultDto, Member, Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { BiographyStyle } from '@/types';
import i18n from '@/plugins/i18n';
import { useNotificationStore } from './notification.store';
import { err } from '@/types';

export const useAIBiographyStore = defineStore('aiBiography', {
  state: () => ({
    loading: false,
    error: null as string | null,
    biographyResult: null as BiographyResultDto | null,
    memberId: null as string | null,
    currentMember: null as Member | null,
    style: BiographyStyle.Emotional as BiographyStyle,
    generatedFromDB: true,
    userPrompt: null as string | null,
    language: 'Vietnamese',
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
          this.language, // Pass language parameter
        );

        if (result.ok) {
          this.biographyResult = { content: result.value };
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

    async saveBiography(memberId: string, content: string): Promise<Result<void, ApiError>> {
      if (!memberId || !content) {
        this.error = i18n.global.t('aiBiography.errors.saveFailed');
        return err({ message: this.error } as ApiError); // Return an error result
      }

      this.loading = true;
      this.error = null;

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
      this.loading = false;
      return result;
    },
  },
});
