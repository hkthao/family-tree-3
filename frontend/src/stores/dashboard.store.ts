import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { DashboardData } from '@/types';

export const useDashboardStore = defineStore('dashboard', {
  state: () => ({
    dashboardData: {
      stats: null,
    } as DashboardData,
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async fetchDashboardStats(familyId?: string) {
      this.loading = true;
      this.error = null;
      try {
        const response = await this.services.dashboard.fetchStats(familyId);
        if (response.ok) {
          this.dashboardData.stats = response.value;
        } else {
          this.error = response.error?.message || 'Failed to fetch dashboard stats.';
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred while fetching stats.';
      } finally {
        this.loading = false;
      }
    },

    async fetchAllDashboardData(familyId?: string) {
      this.loading = true;
      this.error = null;
      try {
        await Promise.all([
          this.fetchDashboardStats(familyId),
        ]);
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred while fetching all dashboard data.';
      } finally {
        this.loading = false;
      }
    },
  },
});
