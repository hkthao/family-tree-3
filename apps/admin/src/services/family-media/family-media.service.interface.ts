import type { Result } from '@/types';
import type { Paginated, FamilyMedia, FamilyMediaFilter, FamilyMediaAddFromUrlDto, ListOptions } from '@/types';

export interface IFamilyMediaService {
  search(
    listOptions: ListOptions,
    filters: FamilyMediaFilter,
  ): Promise<Result<Paginated<FamilyMedia>>>;
  getById(id: string): Promise<Result<FamilyMedia>>;
  create(familyId: string, file: File, description?: string): Promise<Result<FamilyMedia>>; 
  addFromUrl(familyId: string, payload: FamilyMediaAddFromUrlDto): Promise<Result<FamilyMedia>>; // New method
  delete(id: string): Promise<Result<boolean>>;
}
