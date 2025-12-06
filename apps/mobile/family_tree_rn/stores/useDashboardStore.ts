import { create } from 'zustand';
import { getPublicDashboardData, DashboardMetrics } from '@/src/api/publicApiClient';

interface DashboardState {
  dashboardData: DashboardMetrics | null;
  loading: boolean;
  error: string | null;
  getDashboardData: (familyId: string) => Promise<void>;
}

export const useDashboardStore = create<DashboardState>((set) => {
  return {
    dashboardData: null,
    loading: false,
    error: null,

    getDashboardData: async (familyId: string) => {
      set({ loading: true, error: null });
      try {
        const data = await getPublicDashboardData(familyId);
        set({ dashboardData: data, loading: false });
      } catch (err: any) {
        set({ error: err.message || 'Failed to fetch dashboard data', loading: false });
      }
    },
  };
});
