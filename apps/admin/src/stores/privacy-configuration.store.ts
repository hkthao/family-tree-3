import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface PrivacyConfiguration {
  id: string;
  familyId: string;
  publicMemberProperties: string[];
}

export const usePrivacyConfigurationStore = defineStore('privacyConfiguration', {
  state: () => ({
    privacyConfig: null as PrivacyConfiguration | null,
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async fetchPrivacyConfiguration(familyId: string): Promise<Result<PrivacyConfiguration, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.privacyConfiguration.get(familyId);
      if (result.ok) {
        this.privacyConfig = result.value;
      } else {
        this.error = i18n.global.t('privacyConfiguration.errors.fetch');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async updatePrivacyConfiguration(familyId: string, publicMemberProperties: string[]): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.privacyConfiguration.update(familyId, publicMemberProperties);
      if (result.ok) {
        // Optionally refetch or update state directly
        if (this.privacyConfig && this.privacyConfig.familyId === familyId) {
          this.privacyConfig.publicMemberProperties = publicMemberProperties;
        }
      } else {
        this.error = i18n.global.t('privacyConfiguration.errors.update');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    clearPrivacyConfiguration() {
      this.privacyConfig = null;
    },
  },
});
