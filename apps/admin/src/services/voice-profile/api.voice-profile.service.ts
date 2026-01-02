
import type { IVoiceProfileService } from './voice-profile.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Result } from '@/types';
import type { VoiceProfile, CreateVoiceProfileCommand, UpdateVoiceProfileCommand } from '@/types';
import { ApiCrudService } from '../common/api.crud.service';
export class ApiVoiceProfileService extends ApiCrudService<VoiceProfile, CreateVoiceProfileCommand, UpdateVoiceProfileCommand> implements IVoiceProfileService {
  constructor(apiClient: ApiClientMethods) {
    super(apiClient, '/voice-profiles');
  }

  async createVoiceProfile(
    memberId: string,
    command: CreateVoiceProfileCommand
  ): Promise<Result<VoiceProfile>> {
    return this.api.post<VoiceProfile>(
        `/members/${memberId}/voice-profiles`,
        command
      );
  }


  async exportVoiceProfiles(memberId: string): Promise<Result<VoiceProfile[]>> {
    return this.api.get<VoiceProfile[]>(`/members/${memberId}/voice-profiles/export`);
  }

  async importVoiceProfiles(memberId: string, data: VoiceProfile[]): Promise<Result<void>> {
    return this.api.post(`/members/${memberId}/voice-profiles/import`, data);
  }

}
