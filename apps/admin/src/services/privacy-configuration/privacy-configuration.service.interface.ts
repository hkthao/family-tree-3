import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { PrivacyConfiguration } from '@/stores/privacy-configuration.store';

export interface IPrivacyConfigurationService {
  get(familyId: string): Promise<Result<PrivacyConfiguration, ApiError>>;
  update(familyId: string, publicMemberProperties: string[]): Promise<Result<void, ApiError>>;
}
