import { defineStore } from 'pinia';
import type { RecentActivity, TargetType } from '@/types';

export const useUserActivityStore = defineStore('userActivity', {
  state: () => ({
    items: [] as RecentActivity[],
    loading: false,
    error: null as string | null,
    page: 1,
    pageSize: 10,
    totalPages: 0,
    totalItems: 0,
  }),
  actions: {
    async fetchRecentActivities(pageSize = 10, targetType?: TargetType, targetId?: string, groupId?: string, page = 1) {
      this.loading = true;
      this.error = null;
      try {
        const response = await this.services.userActivity.getRecentActivities(page, pageSize, targetType, targetId, groupId);
        if (response.ok) {
          this.items = response.value.items;
          this.page = response.value.page;
          this.totalPages = response.value.totalPages;
          this.totalItems = response.value.totalItems;
        } else {
          this.error = response.error?.message || 'Failed to fetch recent activities.';
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred while fetching recent activities.';
      } finally {
        this.loading = false;
      }
    },
  },
});
