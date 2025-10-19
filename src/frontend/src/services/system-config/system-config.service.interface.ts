import { type Result, type ApiError, type SystemConfig } from '@/types';

export interface ISystemConfigService {
  getSystemConfigs(): Promise<Result<SystemConfig[], ApiError>>;
  updateSystemConfig(key: string, value: any): Promise<Result<SystemConfig, ApiError>>;
}