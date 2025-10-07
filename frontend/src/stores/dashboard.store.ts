import { defineStore } from 'pinia';
import { ref } from 'vue';
import { ok, err } from '@/types/common/result';
import type { Result } from '@/types/common/result';
import type { DashboardData, DashboardStats, RecentActivityItem, UpcomingEvent } from '@/types/dashboard';
import type { Event as AppEvent } from '@/types';
import { useEventStore } from '@/stores/event.store';

// Mock API Service (replace with actual API service later)
const mockDashboardApi = {
  fetchStats: async (familyId?: string): Promise<Result<DashboardStats>> => {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay
    console.log('Fetching stats for family:', familyId);
    return ok({
      totalFamilies: 12,
      totalMembers: 150,
      totalRelationships: 300,
      totalGenerations: 7,
    });
  },

  fetchRecentActivity: async (familyId?: string): Promise<Result<RecentActivityItem[]>> => {
    await new Promise(resolve => setTimeout(resolve, 600));
    console.log('Fetching recent activity for family:', familyId);
    const allActivities: RecentActivityItem[] = [
      { id: '1', type: 'member', description: 'Added John Doe', timestamp: new Date().toISOString(), familyId: 'family1' },
      { id: '2', type: 'relationship', description: 'Linked Jane Doe to John Doe', timestamp: new Date(Date.now() - 3600000).toISOString(), familyId: 'family1' },
      { id: '3', type: 'family', description: 'Created Smith Family', timestamp: new Date(Date.now() - 7200000).toISOString(), familyId: 'family2' },
      { id: '4', type: 'member', description: 'Updated profile of Alice Smith', timestamp: new Date(Date.now() - 10800000).toISOString(), familyId: 'family1' },
      { id: '5', type: 'relationship', description: 'Added child to Bob Johnson', timestamp: new Date(Date.now() - 14400000).toISOString(), familyId: 'family2' },
    ];

    if (familyId) {
      return ok(allActivities.filter(item => item.familyId === familyId));
    }

    return ok(allActivities);
  },
};

export const useDashboardStore = defineStore('dashboard', () => {
  const eventStore = useEventStore(); // Initialize event store

  const dashboardData = ref<DashboardData>({
    stats: null,
    recentActivity: [],
    upcomingEvents: [], // Changed from upcomingBirthdays
  });
  const loading = ref(false);
  const error = ref<string | null>(null);

  const fetchDashboardStats = async (familyId?: string) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await mockDashboardApi.fetchStats(familyId);
      if (response.ok) {
        dashboardData.value.stats = response.value;
      } else {
        error.value = response.error?.message || 'Failed to fetch dashboard stats.';
      }
    } catch (err: any) {
      error.value = err.message || 'An unexpected error occurred while fetching stats.';
    } finally {
      loading.value = false;
    }
  };

  const fetchRecentActivity = async (familyId?: string) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await mockDashboardApi.fetchRecentActivity(familyId);
      if (response.ok) {
        dashboardData.value.recentActivity = response.value;
      } else {
        error.value = response.error?.message || 'Failed to fetch recent activity.';
      }
    } catch (err: any) {
      error.value = err.message || 'An unexpected error occurred while fetching recent activity.';
    } finally {
      loading.value = false;
    }
  };

  const fetchUpcomingEvents = async (familyId?: string) => {
    loading.value = true;
    error.value = null;
    try {
      // Set filter for upcoming events (e.g., startDate from today)
      eventStore.filter = {
        ...eventStore.filter,
        familyId: familyId,
        startDate: new Date(), // Set startDate as a Date object
      };
      await eventStore._loadItems(); // Use internal loadItems for filtering

      if (!eventStore.error) {
        dashboardData.value.upcomingEvents = eventStore.items as UpcomingEvent[];
      } else {
        error.value = eventStore.error || 'Failed to fetch upcoming events.';
      }
    } catch (err: any) {
      error.value = err.message || 'An unexpected error occurred while fetching upcoming events.';
    } finally {
      loading.value = false;
    }
  };

  const fetchAllDashboardData = async (familyId?: string) => {
    loading.value = true;
    error.value = null;
    try {
      await Promise.all([
        fetchDashboardStats(familyId),
        fetchRecentActivity(familyId),
        fetchUpcomingEvents(familyId),
      ]);
    } catch (err: any) {
      error.value = err.message || 'An unexpected error occurred while fetching all dashboard data.';
    } finally {
      loading.value = false;
    }
  };

  return {
    dashboardData,
    loading,
    error,
    fetchDashboardStats,
    fetchRecentActivity,
    fetchUpcomingEvents,
    fetchAllDashboardData,
  };
});