import { create } from 'zustand';
import { dashboardService } from '@/services'; // Import the new dashboardService
import { DashboardMetrics } from '@/types';

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
        const result = await dashboardService.getDashboardData(familyId);
        if (result.isSuccess && result.value) {
          set({ dashboardData: result.value, loading: false });
        } else {
          set({ error: result.error?.message || 'Failed to fetch dashboard data', loading: false });
        }
      } catch (err: any) {
        set({ error: err.message || 'Failed to fetch dashboard data', loading: false });
      }
    },
  };
});
