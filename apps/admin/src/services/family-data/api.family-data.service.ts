import type { IFamilyDataService } from './family-data.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Result } from '@/types/result'; // Only import Result as a type
import type { ApiError } from '@/types/api-error'; // ApiError is a class, so import as type
import type { FamilyExportDto } from '@/types/family';

export class ApiFamilyDataService implements IFamilyDataService {
  constructor(private api: ApiClientMethods) {}

  async exportFamilyData(familyId: string): Promise<Result<FamilyExportDto, ApiError>> {
    return this.api.get<FamilyExportDto>(`/api/family-data/${familyId}/export`);
  }

  async importFamilyData(familyData: FamilyExportDto): Promise<Result<string, ApiError>> {
    return this.api.post<string>('/api/family-data/import', familyData);
  }
}