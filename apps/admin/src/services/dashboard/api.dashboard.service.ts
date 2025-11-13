import type { IDashboardService } from './dashboard.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, DashboardStats } from '@/types';

export class ApiDashboardService implements IDashboardService {
  constructor(private http: ApiClientMethods) {}

  async fetchStats(familyId?: string): Promise<Result<DashboardStats, ApiError>> {
    const params = new URLSearchParams();
    if (familyId) params.append('familyId', familyId);

    return this.http.get<DashboardStats>(`/dashboard/stats?${params.toString()}`);
  }
}
