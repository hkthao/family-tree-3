import type { IFamilyEventService } from './family-event.service.interface';
import type { FamilyEvent } from '@/types/family-event';
import type { Paginated } from '@/types/pagination';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiFamilyEventService implements IFamilyEventService {
  private apiUrl = `${API_BASE_URL}/family-events`;

  async fetch(): Promise<FamilyEvent[]> {
    console.log('Fetching family events from API');
    return [];
  }

  async getById(id: string): Promise<FamilyEvent | undefined> {
    console.log(`Fetching family event ${id} from API`);
    return undefined;
  }

  async add(newItem: Omit<FamilyEvent, 'id'>): Promise<FamilyEvent> {
    console.log('Adding family event via API');
    throw new Error('Not implemented');
  }

  async update(updatedItem: FamilyEvent): Promise<FamilyEvent> {
    console.log(`Updating family event ${updatedItem.id} via API`);
    throw new Error('Not implemented');
  }

  async delete(id: string): Promise<void> {
    console.log(`Deleting family event ${id} via API`);
    throw new Error('Not implemented');
  }

  async searchFamilyEvents(
    searchQuery: string,
    familyId?: string,
    page: number = 1,
    itemsPerPage: number = 10
  ): Promise<Paginated<FamilyEvent>> {
    console.log('Searching family events via API');
    return { items: [], totalItems: 0, totalPages: 0 };
  }
}
