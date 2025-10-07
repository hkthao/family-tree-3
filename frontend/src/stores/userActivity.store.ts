import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { RecentActivity, TargetType } from '@/types';

interface UserActivityState {
  items: RecentActivity[];
  loading: boolean;
  error: string | null;
}

export const useUserActivityStore = defineStore('userActivity', {
  state: () => ({
    items: [] as RecentActivity[],
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async fetchRecentActivities(limit?: number, targetType?: TargetType, targetId?: string, familyId?: string) {
      this.loading = true;
      this.error = null;
      try {
        const response = await this.services.userActivity.getRecentActivities(limit, targetType, targetId, familyId);
        if (response.ok) {
          this.items = response.value;
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
