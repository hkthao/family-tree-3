import type { FamilyDict, Result, FamilyDictFilter, FamilyDictImport, Paginated } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IFamilyDictService extends ICrudService<FamilyDict> {
  loadItems(
    filters: FamilyDictFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<FamilyDict>, ApiError>>;

  importItems(data: FamilyDictImport): Promise<Result<string[], ApiError>>;
}