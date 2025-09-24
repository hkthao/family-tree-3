import type { FamilyEvent } from '@/types/family-event';
import type { Paginated } from '@/types/pagination';
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types/result';
import type { ApiError } from '@/utils/api';

export interface EventFilter {
  searchQuery?: string;
  type?: 'Birth' | 'Marriage' | 'Death' | 'Migration' | 'Other';
  familyId?: string | null ;
  startDate?: Date | null;
  endDate?: Date | null;
  location?: string;
}

export interface IFamilyEventService extends ICrudService<FamilyEvent> { // Extend ICrudService
  searchItems(
    filters: EventFilter,
    page?: number,
    itemsPerPage?: number
  ): Promise<Result<Paginated<FamilyEvent>, ApiError>>
}
