import type { ApiError, FamilyDto, PrivacyConfiguration, Result, FamilyAddDto, FamilyUpdateDto, ListOptions, FilterOptions, Paginated, FamilyLimitConfiguration, IFamilyAccess, FamilyImportDto, IFamilyTreeData } from "@/types";
import type { ICrudService } from "../common/crud.service.interface";

export interface IFamilyService extends ICrudService<FamilyDto, FamilyAddDto, FamilyUpdateDto> {
  addItems(newItems: FamilyAddDto[]): Promise<Result<string[], ApiError>>;
  exportFamilyPdf(familyId: string, htmlContent: string): Promise<Result<Blob, ApiError>>;
  getPrivacyConfiguration(familyId: string): Promise<Result<PrivacyConfiguration, ApiError>>;
  updatePrivacyConfiguration(familyId: string, settings: PrivacyConfiguration): Promise<Result<void, ApiError>>;
  getUserFamilyAccess(): Promise<Result<IFamilyAccess[], ApiError>> ;
  searchPublic(listOptions: ListOptions, filterOptions: FilterOptions): Promise<Result<Paginated<FamilyDto>, ApiError>>;
  getFamilyLimitConfiguration(familyId: string): Promise<Result<FamilyLimitConfiguration, ApiError>>;
  updateFamilyLimitConfiguration(familyId: string, payload: { maxMembers: number; maxStorageMb: number; aiChatMonthlyLimit: number }): Promise<Result<void, ApiError>>;
  importFamilyData(familyData: FamilyImportDto, clearExistingData: boolean): Promise<Result<string, ApiError>>;
  fetchFamilyTreeData(familyId: string, initialMemberId: string | null): Promise<Result<IFamilyTreeData, ApiError>>;
}


