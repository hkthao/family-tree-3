import type { ApiError, FamilyLinkRequestDto, FamilyLinkRequestFilter, Paginated, Result } from '@/types';

export interface IFamilyLinkRequestService {
  searchFamilyLinkRequests(familyId: string, filters: FamilyLinkRequestFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<FamilyLinkRequestDto>, ApiError>>;
  getFamilyLinkRequestById(id: string): Promise<Result<FamilyLinkRequestDto, ApiError>>;
  createFamilyLinkRequest(requestingFamilyId: string, targetFamilyId: string, requestMessage?: string): Promise<Result<string, ApiError>>;
  deleteFamilyLinkRequest(id: string): Promise<Result<void, ApiError>>;
  approveFamilyLinkRequest(requestId: string, responseMessage?: string): Promise<Result<void, ApiError>>;
  rejectFamilyLinkRequest(requestId: string, responseMessage?: string): Promise<Result<void, ApiError>>;
}