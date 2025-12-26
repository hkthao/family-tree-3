import type {
  FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto,
} from '@/types';
import type { ICrudService } from '@/services/common/crud.service.interface'; // Updated import

export interface IFamilyLocationService extends ICrudService<FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto> {
  // FamilyLocationService will use the search, getById, add, update, delete, getByIds from ICrudService
  // Add any specific methods not covered by ICrudService here if needed.
}
