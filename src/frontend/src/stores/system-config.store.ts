import { defineStore } from 'pinia';
import { type SystemConfig } from '@/types';
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

    async updateSystemConfig(id: string, value: SystemConfig) {
      this.loading = true;
      this.error = null;
      const result = await this.services.systemConfig.updateSystemConfig(
        id,
        value,
      );
      if (!result.ok) {
        // Revert on error
        this.error = i18n.global.t('systemConfig.errors.update');
        console.error(result.error);
      }
      this.loading = false;
    },
  },
});
