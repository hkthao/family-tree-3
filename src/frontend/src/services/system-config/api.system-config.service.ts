import type { ISystemConfigService } from './system-config.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { type Result, type SystemConfig } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiSystemConfigService implements ISystemConfigService {
  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/SystemConfiguration`;

  async getSystemConfigs(): Promise<Result<SystemConfig[], ApiError>> {
    return this.http.get<SystemConfig[]>(this.apiUrl);
  }

  async updateSystemConfig(
    id: string,
    value: any,
  ): Promise<Result<SystemConfig, ApiError>> {
    return this.http.put<SystemConfig>(`${this.apiUrl}/${id}`, value);
  }
}
