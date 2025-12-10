import type { Result } from '@/types/result';
import type { PaginatedList, FamilyMedia, MediaLink, FamilyMediaFilter } from '@/types';
import type { AxiosInstance } from 'axios'; // Assuming Axios is used by ApiClientMethods

export interface IFamilyMediaService {
  getFamilyMediaList(familyId: string, filters: FamilyMediaFilter): Promise<Result<PaginatedList<FamilyMedia>>>;
  getFamilyMediaById(familyId: string, id: string): Promise<Result<FamilyMedia>>;
  createFamilyMedia(familyId: string, file: File, description?: string): Promise<Result<string>>; // Returns ID of created media
  deleteFamilyMedia(familyId: string, id: string): Promise<Result<boolean>>;
  linkMediaToEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<string>>; // Returns ID of MediaLink
  unlinkMediaFromEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<boolean>>;
  getMediaLinksByFamilyMediaId(familyId: string, familyMediaId: string): Promise<Result<MediaLink[]>>;
  getMediaLinksByRefId(familyId: string, refType: string, refId: string): Promise<Result<MediaLink[]>>;
}
