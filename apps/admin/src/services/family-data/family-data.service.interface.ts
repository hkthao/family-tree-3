import type { Result } from '@/types/result';
import type { ApiError } from '@/types/api-error';
import type { FamilyExportDto } from '@/types/family'; // Assuming FamilyExportDto is defined here

export interface IFamilyDataService {
  exportFamilyData(familyId: string): Promise<Result<FamilyExportDto, ApiError>>;
  importFamilyData(familyData: FamilyExportDto): Promise<Result<string, ApiError>>; // Returns new family ID
}
