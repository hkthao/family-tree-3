import { defineStore } from 'pinia';
import { type SystemConfig, type ApiError } from '@/types';
import i18n from '@/plugins/i18n';

export const useSystemConfigStore = defineStore('systemConfig', {
  state: () => ({
    configs: [] as SystemConfig[],
    loading: false,
    error: null as string | null,
  }),
  getters: {
    getSystemConfigByKey: (state) => (key: string) => {
      return state.configs.find((config) => config.key === key);
    },
  },
  actions: {
    async fetchSystemConfigs() {
      this.loading = true;
      this.error = null;
      const result = await this.services.systemConfig.getSystemConfigs();
      if (result.ok) {
        this.configs = result.value;
      } else {
        this.error = i18n.global.t('systemConfig.errors.fetch');
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateSystemConfig(key: string, value: any) {
      this.loading = true;
      this.error = null;
      // Optimistic update
      const originalConfig = this.configs.find((config) => config.key === key);
      if (originalConfig) {
        const originalValue = originalConfig.value;
        originalConfig.value = value;

        const result = await this.services.systemConfig.updateSystemConfig(key, value);
        if (result.ok) {
          // Update successful, replace with the actual updated config from the backend
          const index = this.configs.findIndex((config) => config.key === key);
          if (index !== -1) {
            this.configs[index] = result.value;
          }
        } else {
          // Revert on error
          originalConfig.value = originalValue;
          this.error = i18n.global.t('systemConfig.errors.update');
          console.error(result.error);
        }
      } else {
        this.error = i18n.global.t('systemConfig.errors.notFound');
      }
      this.loading = false;
    },
  },
});