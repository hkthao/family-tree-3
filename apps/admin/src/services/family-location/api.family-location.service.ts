import type { ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '@/services/common/api.crud.service';
import type { IFamilyLocationService } from './family-location.service.interface';
import type { FamilyLocation } from '@/types';

export class ApiFamilyLocationService extends ApiCrudService<FamilyLocation> implements IFamilyLocationService {
  constructor(apiClient: ApiClientMethods) {
    super(apiClient, 'family-locations');
  }
  // Implement other specific methods here if any
}
