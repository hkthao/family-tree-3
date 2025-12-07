// apps/mobile/family_tree_rn/services/dashboard/dashboard.service.interface.ts

import { Result, DashboardMetrics } from '@/types';

export interface IDashboardService {
  getDashboardData(familyId: string): Promise<Result<DashboardMetrics>>;
}
