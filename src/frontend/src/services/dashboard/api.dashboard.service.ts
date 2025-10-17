import type { IDashboardService } from './dashboard.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, DashboardStats } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiDashboardService implements IDashboardService {
  private apiUrl = `${API_BASE_URL}/dashboard`;

  constructor(private http: ApiClientMethods) {}

  async fetchStats(familyId?: string): Promise<Result<DashboardStats, ApiError>> {
    const params = new URLSearchParams();
    if (familyId) params.append('familyId', familyId);

    return this.http.get<DashboardStats>(`${this.apiUrl}/stats?${params.toString()}`);
  }
}
