import type { ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '@/services/common/api.crud.service';
import type { IFamilyLocationService } from './family-location.service.interface';
import type { FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto } from '@/types';

export class ApiFamilyLocationService extends ApiCrudService<FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto> implements IFamilyLocationService {
  constructor(apiClient: ApiClientMethods) {
    super(apiClient, 'family-locations');
  }

  async exportFamilyLocations(familyId: string): Promise<Result<FamilyLocation[], ApiError>> {
    return this.apiClient.get<FamilyLocation[]>(`${this.basePath}/export/${familyId}`);
  }

  async importFamilyLocations(familyId: string, locations: FamilyLocation[]): Promise<Result<FamilyLocation[], ApiError>> {
    return this.apiClient.post<FamilyLocation[]>(`${this.basePath}/import/${familyId}`, locations);
  }
}
