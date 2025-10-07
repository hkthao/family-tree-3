import { defineStore } from 'pinia';
import type { DashboardStats, Event } from '@/types';

export const useDashboardStore = defineStore('dashboard', {
  state: () => ({
    stats: null as DashboardStats | null,
    upcomingEvents: [] as Event[],
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
          this.stats = response.value;
        } else {
          this.error =
            response.error?.message || 'Failed to fetch dashboard stats.';
        }
      } catch (err: any) {
        this.error =
          err.message || 'An unexpected error occurred while fetching stats.';
      } finally {
        this.loading = false;
      }
    },

    async fetchUpcomingEvents(familyId?: string) {
      this.loading = true;
      this.error = null;
      try {
        const response = await this.services.event.getUpcomingEvents(familyId);
        if (response.ok) this.upcomingEvents = response.value as Event[];
        else {
          this.error =
            response.error?.message || 'Failed to fetch upcoming events.';
        }
      } catch (err: any) {
        this.error =
          err.message ||
          'An unexpected error occurred while fetching upcoming events.';
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
          this.fetchUpcomingEvents(familyId),
        ]);
      } catch (err: any) {
        this.error =
          err.message ||
          'An unexpected error occurred while fetching all dashboard data.';
      } finally {
        this.loading = false;
      }
    },
  },
});
