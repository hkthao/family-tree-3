import type { ApiError } from '@/plugins/axios';
import type { FamilyLinkRequestDto, FamilyLinkRequestFilter, Paginated, Result, UpdateFamilyLinkRequestCommand } from '@/types';

export interface IFamilyLinkRequestService {
  searchFamilyLinkRequests(familyId: string, filters: FamilyLinkRequestFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<FamilyLinkRequestDto>, ApiError>>;
  getFamilyLinkRequestById(id: string): Promise<Result<FamilyLinkRequestDto, ApiError>>;
  createFamilyLinkRequest(requestingFamilyId: string, targetFamilyId: string): Promise<Result<string, ApiError>>;
  updateFamilyLinkRequest(command: UpdateFamilyLinkRequestCommand): Promise<Result<void, ApiError>>;
  deleteFamilyLinkRequest(id: string): Promise<Result<void, ApiError>>;
  approveFamilyLinkRequest(requestId: string): Promise<Result<void, ApiError>>;
  rejectFamilyLinkRequest(requestId: string): Promise<Result<void, ApiError>>;
}