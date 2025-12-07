// apps/mobile/family_tree_rn/services/family/family.service.interface.ts

import { Result } from '@/types';
import { FamilyDetailDto, PaginatedList, FamilyListDto, SearchPublicFamiliesQuery } from '@/types';

export interface IFamilyService {
  getFamilyById(id: string): Promise<Result<FamilyDetailDto>>;
  searchFamilies(query: SearchPublicFamiliesQuery): Promise<Result<PaginatedList<FamilyListDto>>>;
}
