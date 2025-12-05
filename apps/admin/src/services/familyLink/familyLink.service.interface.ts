import type { ApiError } from '@/plugins/axios';
import type { FamilyLinkDto, FamilyLinkFilter, Paginated, Result } from '@/types';

export interface IFamilyLinkService {
  getFamilyLinks(familyId: string, filters: FamilyLinkFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<FamilyLinkDto>, ApiError>>;
  getFamilyLinkById(familyLinkId: string): Promise<Result<FamilyLinkDto, ApiError>>;
  deleteFamilyLink(familyLinkId: string): Promise<Result<void, ApiError>>;
}