import type { Family, FamilyFilter, Result, Paginated } from "@/types";
import type { ApiError } from "@/plugins/axios";
import type { ICrudService } from "../common/crud.service.interface";
import type { FamilyExportDto, IFamilyAccess, GenerateFamilyDataCommand } from '@/types/family';
import type { AnalyzedDataDto } from '@/types/ai';
import type { PrivacyConfiguration } from '@/stores/privacy-configuration.store';

export interface IFamilyService extends ICrudService<Family> {
  addItems(newItems: Omit<Family, 'id'>[]): Promise<Result<string[], ApiError>>;
  exportFamilyData(familyId: string): Promise<Result<FamilyExportDto, ApiError>>;
  importFamilyData(familyId: string, familyData: FamilyExportDto, clearExistingData: boolean): Promise<Result<string, ApiError>>;
  exportFamilyPdf(familyId: string, htmlContent: string): Promise<Result<Blob, ApiError>>;
  getPrivacyConfiguration(familyId: string): Promise<Result<PrivacyConfiguration, ApiError>>;
  updatePrivacyConfiguration(familyId: string, publicMemberProperties: string[]): Promise<Result<void, ApiError>>;
  getUserFamilyAccess(): Promise<Result<IFamilyAccess[], ApiError>> ;
  generateFamilyData(command: GenerateFamilyDataCommand): Promise<Result<AnalyzedDataDto, ApiError>>;
}

