
import type { IVoiceProfileService } from './voice-profile.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Result, VoiceGenerationDto } from '@/types';
import type { VoiceProfile, CreateVoiceProfileCommand, UpdateVoiceProfileCommand, PreprocessAndCreateVoiceProfileDto, VoiceProfileDto } from '@/types';
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
        `/voice-profiles/${memberId}`,
        command
      );
  }

  async preprocessAndCreate(
    command: PreprocessAndCreateVoiceProfileDto
  ): Promise<Result<VoiceProfileDto>> {
    return this.api.post<VoiceProfileDto>(
      `/voice-profiles/preprocess-and-create`,
      command
    );
  }

  async exportVoiceProfiles(familyId: string): Promise<Result<VoiceProfile[]>> {
    return this.api.get<VoiceProfile[]>(`/voice-profiles/${familyId}/export`);
  }

  async importVoiceProfiles(familyId: string, data: VoiceProfile[]): Promise<Result<void>> {
    return this.api.post(`/voice-profiles/${familyId}/import`, data);
  }

  async generateVoice(voiceProfileId: string, text: string): Promise<Result<VoiceGenerationDto>> {
    return this.api.post<VoiceGenerationDto>(`/voice-profiles/${voiceProfileId}/generate`, { voiceProfileId, text });
  }

  async getVoiceGenerationHistory(voiceProfileId: string): Promise<Result<VoiceGenerationDto[]>> {
    return this.api.get<VoiceGenerationDto[]>(`/voice-profiles/${voiceProfileId}/history`);
  }

}
