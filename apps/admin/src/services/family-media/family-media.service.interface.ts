import type { Result } from '@/types';
import type { Paginated, FamilyMedia, FamilyMediaFilter, FamilyMediaAddFromUrlDto } from '@/types'; // Added FamilyMediaAddFromUrlDto

export interface IFamilyMediaService {
  search(
    familyId: string,
    filters: FamilyMediaFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: { key: string; order: string }[],
  ): Promise<Result<Paginated<FamilyMedia>>>;
  getById(id: string): Promise<Result<FamilyMedia>>;
  create(familyId: string, file: File, description?: string): Promise<Result<FamilyMedia>>; 
  addFromUrl(familyId: string, payload: FamilyMediaAddFromUrlDto): Promise<Result<FamilyMedia>>; // New method
  delete(id: string): Promise<Result<boolean>>;
}
