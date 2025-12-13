import type { ApiError, Result } from '@/types';
import type { DashboardStats } from '@/types';

export interface IDashboardService {
  fetchStats(familyId?: string): Promise<Result<DashboardStats, ApiError>>;
}
