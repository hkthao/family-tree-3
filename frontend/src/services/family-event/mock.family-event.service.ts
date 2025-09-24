import type { IFamilyEventService, EventFilter } from './family-event.service.interface';
import type { FamilyEvent } from '@/types/family';
import type { Paginated } from '@/types/common';
import { generateMockFamilyEvents } from '@/data/mock/family-event.mock';

import { simulateLatency } from '@/utils/mockUtils';
import type { Result } from '@/types/common';
import { ok, err } from '@/types/common';
import type { ApiError } from '@/utils/api';

export class MockFamilyEventService implements IFamilyEventService {
  private _familyEvents: FamilyEvent[] = generateMockFamilyEvents(50);

  get familyEvents(): FamilyEvent[] {
    return [...this._familyEvents];
  }

  async fetch(): Promise<Result<FamilyEvent[], ApiError>> {
    try {
      const events = await simulateLatency(this.familyEvents);
      return ok(events);
    } catch (e) {
      return err({ message: 'Failed to fetch family events from mock service.', details: e as Error });
    }
  }

  async getById(id: string): Promise<Result<FamilyEvent | undefined, ApiError>> {
    try {
      const event = await simulateLatency(this.familyEvents.find((event) => event.id === id));
      return ok(event);
    } catch (e) {
      return err({ message: `Failed to get family event with ID ${id} from mock service.`, details: e as Error });
    }
  }

  async add(newItem: Omit<FamilyEvent, 'id'>): Promise<Result<FamilyEvent, ApiError>> {
    try {
      const eventToAdd = { ...newItem, id: 'mock-id-' + Math.random().toString(36).substring(7) };
      this._familyEvents.push(eventToAdd);
      const addedEvent = await simulateLatency(eventToAdd);
      return ok(addedEvent);
    } catch (e) {
      return err({ message: 'Failed to add family event to mock service.', details: e as Error });
    }
  }

  async update(updatedItem: FamilyEvent): Promise<Result<FamilyEvent, ApiError>> {
    try {
      const index = this._familyEvents.findIndex((event) => event.id === updatedItem.id);
      if (index !== -1) {
        this._familyEvents[index] = updatedItem;
        const updatedEvent = await simulateLatency(updatedItem);
        return ok(updatedEvent);
      }
      return err({ message: 'Family event not found', statusCode: 404 });
    } catch (e) {
      return err({ message: 'Failed to update family event in mock service.', details: e as Error });
    }
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    try {
      const initialLength = this._familyEvents.length;
      this._familyEvents = this._familyEvents.filter((event) => event.id !== id);
      if (this._familyEvents.length === initialLength) {
        return err({ message: 'Family event not found', statusCode: 404 });
      }
      await simulateLatency(undefined);
      return ok(undefined);
    } catch (e) {
      return err({ message: 'Failed to delete family event from mock service.', details: e as Error });
    }
  }

  async searchItems(
    filters: EventFilter,
    page: number = 1,
    itemsPerPage: number = 10
  ): Promise<Result<Paginated<FamilyEvent>, ApiError>> {
    try {
      let filteredEvents = this.familyEvents;

      if (filters.searchQuery) {
        const lowerCaseSearchQuery = filters.searchQuery.toLowerCase();
        filteredEvents = filteredEvents.filter(
          (event) =>
            event.name.toLowerCase().includes(lowerCaseSearchQuery) ||
            (event.description && event.description.toLowerCase().includes(lowerCaseSearchQuery))
        );
      }

      if (filters.type) {
        filteredEvents = filteredEvents.filter((event) => event.type === filters.type);
      }

      if (filters.familyId) {
        filteredEvents = filteredEvents.filter((event) => event.familyId === filters.familyId);
      }

      if (filters.startDate) {
        filteredEvents = filteredEvents.filter((event) => event.startDate && new Date(event.startDate) >= filters.startDate!);
      }

      if (filters.endDate) {
        filteredEvents = filteredEvents.filter((event) => event.startDate && new Date(event.startDate) <= filters.endDate!);
      }

      if (filters.location) {
        const lowerCaseLocation = filters.location.toLowerCase();
        filteredEvents = filteredEvents.filter((event) => event.location && event.location.toLowerCase().includes(lowerCaseLocation));
      }

      const totalItems = filteredEvents.length;
      const totalPages = Math.ceil(totalItems / itemsPerPage);
      const start = (page - 1) * itemsPerPage;
      const end = start + itemsPerPage;
      const items = filteredEvents.slice(start, end);

      const paginatedResult = await simulateLatency({
        items,
        totalItems,
        totalPages,
      });
      return ok(paginatedResult);
    } catch (e) {
      return err({ message: 'Failed to search family events from mock service.', details: e as Error });
    }
  }
}
