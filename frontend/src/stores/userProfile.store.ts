import { defineStore } from 'pinia';
import type { UserProfile } from '@/types';
import i18n from '@/plugins/i18n';

import { useAuthStore } from './auth.store';

export const useUserProfileStore = defineStore('userProfile', {
  state: () => ({
    loading: false,
    error: null as string | null,
    userProfile: null as UserProfile | null,
    allUserProfiles: [] as UserProfile[],
  }),

  actions: {
    async fetchCurrentUserProfile() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userProfile.getCurrentUserProfile();
        if (result.ok) {
          this.userProfile = result.value;
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.profile.fetchError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.profile.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchUserProfile(id: string) {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userProfile.getUserProfile(id);
        if (result.ok) {
          this.userProfile = result.value;
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.profile.fetchError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.profile.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchUserProfileByExternalId(externalId: string) {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userProfile.getUserProfileByExternalId(externalId);
        if (result.ok) {
          this.userProfile = result.value;
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.profile.fetchError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.profile.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async fetchAllUserProfiles() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userProfile.getAllUserProfiles();
        if (result.ok) {
          this.allUserProfiles = result.value;
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.profile.fetchAllError');
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.profile.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    async updateUserProfile(profile: UserProfile): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.userProfile.updateUserProfile(profile);
        if (result.ok) {
          this.userProfile = result.value;
          return true;
        } else {
          this.error = result.error?.message || i18n.global.t('userSettings.profile.saveError');
          return false;
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('userSettings.profile.unexpectedError');
        return false;
      } finally {
        this.loading = false;
      }
    },

    reset() {
      this.userProfile = null;
      this.allUserProfiles = [];
      this.loading = false;
      this.error = null;
    },
  },
});