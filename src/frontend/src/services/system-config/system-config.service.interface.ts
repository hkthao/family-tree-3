import { type Result, type SystemConfig } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface ISystemConfigService {
  getSystemConfigs(): Promise<Result<SystemConfig[], ApiError>>;
  updateSystemConfig(key: string, value: any): Promise<Result<SystemConfig, ApiError>>;
}