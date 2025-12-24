import type { FamilyDict, FamilyDictImport, AddFamilyDictDto, UpdateFamilyDictDto } from '@/types'; // Changed Paginated to Paginated
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types'; // Added type keyword

export interface IFamilyDictService extends ICrudService<FamilyDict, AddFamilyDictDto, UpdateFamilyDictDto> {
  importItems(data: FamilyDictImport): Promise<Result<string[]>>; // Removed ApiError
}