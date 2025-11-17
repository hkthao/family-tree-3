import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import type { Result } from '@/types';
import type { IPrivacyConfigurationService } from './privacy-configuration.service.interface';
import type { PrivacyConfiguration } from '@/stores/privacy-configuration.store';

export class ApiPrivacyConfigurationService implements IPrivacyConfigurationService {
  constructor(private http: ApiClientMethods) {}

  async get(familyId: string): Promise<Result<PrivacyConfiguration, ApiError>> {
    const result = await this.http.get<PrivacyConfiguration>(`/PrivacyConfiguration/${familyId}`);
    if (result.ok) {
      // Ensure publicMemberProperties is always an array
      result.value.publicMemberProperties = result.value.publicMemberProperties || [];
    }
    return result;
  }

  async update(familyId: string, publicMemberProperties: string[]): Promise<Result<void, ApiError>> {
    const payload = { familyId, publicMemberProperties };
    return this.http.put<void>(`/PrivacyConfiguration/${familyId}`, payload);
  }
}
