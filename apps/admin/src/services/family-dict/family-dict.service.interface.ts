import type { FamilyDict, FamilyDictImport } from '@/types'; // Changed Paginated to Paginated
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types'; // Added type keyword

export interface IFamilyDictService extends ICrudService<FamilyDict> {
  importItems(data: FamilyDictImport): Promise<Result<string[]>>; // Removed ApiError
}