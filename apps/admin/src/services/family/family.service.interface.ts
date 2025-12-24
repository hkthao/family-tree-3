import type { ApiError, FamilyDto, PrivacyConfiguration, Result, FamilyAddDto, FamilyUpdateDto } from "@/types";
import type { ICrudService } from "../common/crud.service.interface";
import type { FamilyExportDto, IFamilyAccess } from '@/types/family';

export interface IFamilyService extends ICrudService<FamilyDto> {
  addItems(newItems: FamilyAddDto[]): Promise<Result<string[], ApiError>>;
  exportFamilyData(familyId: string): Promise<Result<FamilyExportDto, ApiError>>;
  importFamilyData(familyId: string, familyData: FamilyExportDto, clearExistingData: boolean): Promise<Result<string, ApiError>>;
  exportFamilyPdf(familyId: string, htmlContent: string): Promise<Result<Blob, ApiError>>;
  getPrivacyConfiguration(familyId: string): Promise<Result<PrivacyConfiguration, ApiError>>;
  updatePrivacyConfiguration(familyId: string, publicMemberProperties: string[]): Promise<Result<void, ApiError>>;
  getUserFamilyAccess(): Promise<Result<IFamilyAccess[], ApiError>> ;
}

