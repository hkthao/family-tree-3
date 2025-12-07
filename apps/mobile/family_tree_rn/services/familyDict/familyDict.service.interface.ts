// apps/mobile/family_tree_rn/services/familyDict/familyDict.service.interface.ts

import { Result } from '@/types';
import { FamilyDictDto, PaginatedList, FamilyDictFilter } from '@/types';

export interface IFamilyDictService {
  getFamilyDicts(filter: FamilyDictFilter, page: number, itemsPerPage: number): Promise<Result<PaginatedList<FamilyDictDto>>>;
  getFamilyDictById(id: string): Promise<Result<FamilyDictDto>>;
}
