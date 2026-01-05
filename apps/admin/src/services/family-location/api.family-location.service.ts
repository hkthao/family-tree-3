import type { ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '@/services/common/api.crud.service';
import type { IFamilyLocationService } from './family-location.service.interface';
import type { FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto, ImportFamilyLocationItemDto, Result, ApiError } from '@/types';

export class ApiFamilyLocationService extends ApiCrudService<FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto> implements IFamilyLocationService {
  protected basePath: string; // Added explicit declaration
  constructor(protected http: ApiClientMethods) {
    super(http, 'family-locations');
    this.basePath = 'family-locations'; // Explicitly assign in constructor
  }

  async exportFamilyLocations(familyId: string): Promise<Result<FamilyLocation[], ApiError>> {
    return this.http.get<FamilyLocation[]>(`${this.basePath}/export/${familyId}`);
  }

  async importFamilyLocations(familyId: string, locations: ImportFamilyLocationItemDto[]): Promise<Result<FamilyLocation[], ApiError>> {
            return this.http.post<FamilyLocation[]>(`${this.basePath}/import/${familyId}`, { familyId: familyId, locations: locations });  }
}
