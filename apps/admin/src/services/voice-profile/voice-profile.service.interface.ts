import type { Result } from '@/types';
import type { VoiceProfile, CreateVoiceProfileCommand, UpdateVoiceProfileCommand } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

export interface IVoiceProfileService extends ICrudService<VoiceProfile, CreateVoiceProfileCommand, UpdateVoiceProfileCommand> {
  createVoiceProfile(
    memberId: string,
    command: CreateVoiceProfileCommand
  ): Promise<Result<VoiceProfile>>;

  exportVoiceProfiles(memberId: string): Promise<Result<VoiceProfile[]>>;

  importVoiceProfiles(memberId: string, data: VoiceProfile[]): Promise<Result<void>>;

}