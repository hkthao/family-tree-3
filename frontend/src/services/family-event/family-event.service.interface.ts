import type { FamilyEvent } from '@/types/family-event';
import type { Paginated } from '@/types/pagination';
import type { ICrudService } from '../common/crud.service.interface';

export interface IFamilyEventService extends ICrudService<FamilyEvent> {
  searchFamilyEvents(
    searchQuery: string,
    familyId?: string,
    page?: number,
    itemsPerPage?: number
  ): Promise<Paginated<FamilyEvent>>;
}
