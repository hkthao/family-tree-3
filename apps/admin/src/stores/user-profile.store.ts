import { defineStore } from 'pinia';
import type { UserProfile, UpdateUserProfileRequestDto } from '@/types';
import i18n from '@/plugins/i18n';

export const useUserProfileStore = defineStore('userProfile', {
  state: () => ({
    loading: false,
    error: null as string | null,
    userProfile: null as UserProfile | null,
  }),

  actions: {
    async fetchCurrentUserProfile() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.user.getCurrentUserProfile(); // Use services.user
        if (result.ok) {
          this.userProfile = result.value;
        } else {
          this.error = i18n.global.t('userSettings.profile.fetchError');
        }
      } catch (err: any) {
        this.error = i18n.global.t('userSettings.profile.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async updateUserProfile(profile: UpdateUserProfileRequestDto): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.user.updateUserProfile(profile); // Use services.user
        if (result.ok) {
          this.userProfile = result.value;
          return true;
        } else {
          this.error = i18n.global.t('userSettings.profile.saveError');
          return false;
        }
      } catch (err: any) {
        this.error = i18n.global.t('userSettings.profile.unexpectedError');
        return false;
      } finally {
        this.loading = false;
      }
    },

    reset() {
      this.userProfile = null;
      this.loading = false;
      this.error = null;
    },
  },
});