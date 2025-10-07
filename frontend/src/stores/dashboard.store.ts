import { defineStore } from 'pinia';
import { ref } from 'vue';
import { ok, err } from '@/types/common/result';
import type { Result } from '@/types/common/result';
import type { DashboardData, DashboardStats, RecentActivityItem, UpcomingBirthday, SystemInfo } from '@/types/dashboard';

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
    return ok([
      { id: '1', type: 'member', description: 'Added John Doe', timestamp: new Date().toISOString() },
      { id: '2', type: 'relationship', description: 'Linked Jane Doe to John Doe', timestamp: new Date(Date.now() - 3600000).toISOString() },
      { id: '3', type: 'family', description: 'Created Smith Family', timestamp: new Date(Date.now() - 7200000).toISOString() },
      { id: '4', type: 'member', description: 'Updated profile of Alice Smith', timestamp: new Date(Date.now() - 10800000).toISOString() },
      { id: '5', type: 'relationship', description: 'Added child to Bob Johnson', timestamp: new Date(Date.now() - 14400000).toISOString() },
    ]);
  },

  fetchUpcomingBirthdays: async (familyId?: string): Promise<Result<UpcomingBirthday[]>> => {
    await new Promise(resolve => setTimeout(resolve, 700));
    console.log('Fetching upcoming birthdays for family:', familyId);
    const today = new Date();
    const nextMonth = new Date();
    nextMonth.setMonth(today.getMonth() + 1);

    return ok([
      { id: 'm1', name: 'Alice Smith', dateOfBirth: new Date(today.getFullYear(), today.getMonth(), today.getDate() + 5).toISOString(), age: 30, avatar: 'https://randomuser.me/api/portraits/women/1.jpg' },
      { id: 'm2', name: 'Bob Johnson', dateOfBirth: new Date(today.getFullYear(), today.getMonth() + 1, today.getDate() - 2).toISOString(), age: 45, avatar: 'https://randomuser.me/api/portraits/men/1.jpg' },
      { id: 'm3', name: 'Charlie Brown', dateOfBirth: new Date(today.getFullYear(), today.getMonth() + 1, today.getDate() + 10).toISOString(), age: 22, avatar: 'https://randomuser.me/api/portraits/men/2.jpg' },
    ]);
  },

  fetchSystemInfo: async (familyId?: string): Promise<Result<SystemInfo>> => {
    await new Promise(resolve => setTimeout(resolve, 300));
    console.log('Fetching system info for family:', familyId);
    const isOnline = Math.random() > 0.1; // 90% chance of being online
    return ok({
      apiStatus: isOnline ? 'online' : 'offline',
      appVersion: '1.0.0-beta',
      lastSync: new Date(Date.now() - 60000).toISOString(),
      serverTime: new Date().toISOString(),
      requestSuccessRate: Math.floor(Math.random() * 20) + 80, // 80-100%
    });
  },
};

export const useDashboardStore = defineStore('dashboard', () => {
  const dashboardData = ref<DashboardData>({
    stats: null,
    recentActivity: [],
    upcomingBirthdays: [],
    systemInfo: null,
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

  const fetchUpcomingBirthdays = async (familyId?: string) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await mockDashboardApi.fetchUpcomingBirthdays(familyId);
      if (response.ok) {
        dashboardData.value.upcomingBirthdays = response.value;
      } else {
        error.value = response.error?.message || 'Failed to fetch upcoming birthdays.';
      }
    } catch (err: any) {
      error.value = err.message || 'An unexpected error occurred while fetching upcoming birthdays.';
    } finally {
      loading.value = false;
    }
  };

  const fetchSystemInfo = async (familyId?: string) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await mockDashboardApi.fetchSystemInfo(familyId);
      if (response.ok) {
        dashboardData.value.systemInfo = response.value;
      } else {
        error.value = response.error?.message || 'Failed to fetch system info.';
      }
    } catch (err: any) {
      error.value = err.message || 'An unexpected error occurred while fetching system info.';
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
        fetchUpcomingBirthdays(familyId),
        fetchSystemInfo(familyId),
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
    fetchUpcomingBirthdays,
    fetchSystemInfo,
    fetchAllDashboardData,
  };
});
