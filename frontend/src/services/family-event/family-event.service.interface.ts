import type { FamilyEvent } from '@/types/family';
import type { Paginated } from '@/types/common';
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types/common';
import type { ApiError } from '@/utils/api';

export interface EventFilter {
  searchQuery?: string;
  type?: 'Birth' | 'Marriage' | 'Death' | 'Migration' | 'Other';
  eventType?: string;
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
