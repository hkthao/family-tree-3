import type { Result } from '@/types';
import type { PaginatedList, FamilyMedia, MediaLink, FamilyMediaFilter } from '@/types';

export interface IFamilyMediaService {
  search(
    familyId: string,
    filters: FamilyMediaFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: { key: string; order: string }[],
  ): Promise<Result<PaginatedList<FamilyMedia>>>;
  getById(familyId: string, id: string): Promise<Result<FamilyMedia>>;
  create(familyId: string, file: File, description?: string): Promise<Result<string>>; 
  delete(familyId: string, id: string): Promise<Result<boolean>>;
  linkMediaToEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<string>>; 
  unlinkMediaFromEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<boolean>>;
  getMediaLinksByFamilyMediaId(familyId: string, familyMediaId: string): Promise<Result<MediaLink[]>>;
  getMediaLinksByRefId(familyId: string, refType: string, refId: string): Promise<Result<MediaLink[]>>;
}