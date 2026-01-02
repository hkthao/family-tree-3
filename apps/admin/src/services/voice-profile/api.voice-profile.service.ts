
import type { IVoiceProfileService } from './voice-profile.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Result } from '@/types';
import type { VoiceProfile, CreateVoiceProfileCommand, UpdateVoiceProfileCommand, Paginated } from '@/types';
export class ApiVoiceProfileService implements IVoiceProfileService {
  constructor(private apiClient: ApiClientMethods) {}

  async getVoiceProfilesByMemberId(
    memberId: string,
    page: number,
    itemsPerPage: number,
    search?: string,
    sortBy?: string,
    sortOrder?: string
  ): Promise<Result<Paginated<VoiceProfile>>> {
    return this.apiClient.get<Paginated<VoiceProfile>>(
        `/api/members/${memberId}/voice-profiles`,
        {
          params: {
            page,
            pageSize: itemsPerPage,
            search,
            sortBy,
            sortOrder,
          },
        }
      );
  }

  async getVoiceProfileById(memberId: string, id: string): Promise<Result<VoiceProfile>> {
    return this.apiClient.get<VoiceProfile>(`/api/voice-profiles/${id}`);
  }

  async createVoiceProfile(
    memberId: string,
    command: CreateVoiceProfileCommand
  ): Promise<Result<VoiceProfile>> {
    return this.apiClient.post<VoiceProfile>(
        `/api/members/${memberId}/voice-profiles`,
        command
      );
  }

  async updateVoiceProfile(
    memberId: string,
    id: string,
    command: UpdateVoiceProfileCommand
  ): Promise<Result<VoiceProfile>> {
    return this.apiClient.put<VoiceProfile>(`/api/voice-profiles/${id}`, command);
  }

  async deleteVoiceProfile(memberId: string, id: string): Promise<Result<void>> {
    return this.apiClient.delete(`/api/voice-profiles/${id}`);
  }

  async exportVoiceProfiles(memberId: string): Promise<Result<VoiceProfile[]>> {
    return this.apiClient.get<VoiceProfile[]>(`/api/members/${memberId}/voice-profiles/export`);
  }

  async importVoiceProfiles(memberId: string, data: VoiceProfile[]): Promise<Result<void>> {
    return this.apiClient.post(`/api/members/${memberId}/voice-profiles/import`, data);
  }
}
