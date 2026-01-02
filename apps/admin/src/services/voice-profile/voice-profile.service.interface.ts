import type { Result } from '@/types';
import type { VoiceProfile, CreateVoiceProfileCommand, UpdateVoiceProfileCommand, Paginated } from '@/types';
export interface IVoiceProfileService {
  getVoiceProfilesByMemberId(
    memberId: string,
    page: number,
    itemsPerPage: number,
    search?: string,
    sortBy?: string,
    sortOrder?: string
  ): Promise<Result<Paginated<VoiceProfile>>>;

  getVoiceProfileById(memberId: string, id: string): Promise<Result<VoiceProfile>>;

  createVoiceProfile(
    memberId: string,
    command: CreateVoiceProfileCommand
  ): Promise<Result<VoiceProfile>>;

  updateVoiceProfile(
    memberId: string,
    id: string,
    command: UpdateVoiceProfileCommand
  ): Promise<Result<VoiceProfile>>;

  deleteVoiceProfile(memberId: string, id: string): Promise<Result<void>>;

  exportVoiceProfiles(memberId: string): Promise<Result<VoiceProfile[]>>;

  importVoiceProfiles(memberId: string, data: VoiceProfile[]): Promise<Result<void>>;
}
