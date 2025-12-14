import type { Result } from '@/types';
import type { Paginated, FamilyMedia, FamilyMediaFilter } from '@/types';

export interface IFamilyMediaService {
  search(
    familyId: string,
    filters: FamilyMediaFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: { key: string; order: string }[],
  ): Promise<Result<Paginated<FamilyMedia>>>;
  getById(id: string): Promise<Result<FamilyMedia>>;
  create(familyId: string, file: File, description?: string): Promise<Result<string>>; 
  delete(id: string): Promise<Result<boolean>>;
}