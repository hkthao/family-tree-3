import type {
  FamilyLocation,
  FamilyLocationList,
  FamilyLocationFilter,
} from '@/types';
import type { ICrudService } from '@/services/common/crud.service.interface'; // Updated import
import type { Result } from '@/types/result'; // Import Result type

export interface IFamilyLocationService extends ICrudService<FamilyLocation> {
  // FamilyLocationService will use the search, getById, add, update, delete, getByIds from ICrudService
  // Add any specific methods not covered by ICrudService here if needed.
}
