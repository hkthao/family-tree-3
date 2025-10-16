import type { Result } from '@/types';
import type { DashboardStats } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IDashboardService {
  fetchStats(familyId?: string): Promise<Result<DashboardStats, ApiError>>;
}
