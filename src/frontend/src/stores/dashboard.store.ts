import { defineStore } from 'pinia';
import type { DashboardStats, Event, Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export const useDashboardStore = defineStore('dashboard', {
  state: () => ({
    stats: null as DashboardStats | null,
    upcomingEvents: [] as Event[],
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async fetchDashboardStats(familyId?: string): Promise<Result<DashboardStats, ApiError>> {
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
        return response;
      } catch (err: any) {
        this.error =
          err.message || 'An unexpected error occurred while fetching stats.';
        return { ok: false, error: { message: this.error } as ApiError };
      } finally {
        this.loading = false;
      }
    },

    async fetchUpcomingEvents(familyId?: string): Promise<Result<Event[], ApiError>> {
      this.loading = true;
      this.error = null;
      try {
        const response = await this.services.event.getUpcomingEvents(familyId);
        if (response.ok) this.upcomingEvents = response.value as Event[];
        else {
          this.error =
            response.error?.message || 'Failed to fetch upcoming events.';
        }
        return response;
      } catch (err: any) {
        this.error =
          err.message ||
          'An unexpected error occurred while fetching upcoming events.';
        return { ok: false, error: { message: this.error } as ApiError };
      } finally {
        this.loading = false;
      }
    },

    async fetchAllDashboardData(familyId?: string): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      try {
        const [statsResult, eventsResult] = await Promise.all([
          this.fetchDashboardStats(familyId),
          this.fetchUpcomingEvents(familyId),
        ]);

        if (!statsResult.ok) {
          this.error = statsResult.error?.message || 'Failed to fetch dashboard stats.';
          return { ok: false, error: statsResult.error };
        }
        if (!eventsResult.ok) {
          this.error = eventsResult.error?.message || 'Failed to fetch upcoming events.';
          return { ok: false, error: eventsResult.error };
        }
        return { ok: true, value: undefined };
      } catch (err: any) {
        this.error =
          err.message ||
          'An unexpected error occurred while fetching all dashboard data.';
        return { ok: false, error: { message: this.error } as ApiError };
      } finally {
        this.loading = false;
      }
    },
  },
});
