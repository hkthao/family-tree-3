import mockEvents from '@/data/mock/events.json';
import { simulateLatency } from '@/utils/mockUtils';
import type { Result, Event, EventFilter, Paginated } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/utils/api';
import type { IEventService } from './event.service.interface';

export class MockEventService implements IEventService {
  private _events: Event[] = mockEvents as unknown as Event[];

  get events(): Event[] {
    return [...this._events];
  }

  async fetch(): Promise<Result<Event[], ApiError>> {
    try {
      const events = await simulateLatency(this.events);
      return ok(events);
    } catch (e) {
      return err({
        message: 'Failed to fetch events from mock service.',
        details: e as Error,
      });
    }
  }

  async getById(id: string): Promise<Result<Event | undefined, ApiError>> {
    try {
      const event = await simulateLatency(
        this.events.find((event) => event.id === id),
      );
      return ok(event);
    } catch (e) {
      return err({
        message: `Failed to get event with ID ${id} from mock service.`,
        details: e as Error,
      });
    }
  }

  async add(newItem: Omit<Event, 'id'>): Promise<Result<Event, ApiError>> {
    try {
      const eventToAdd = {
        ...newItem,
        id: 'mock-id-' + Math.random().toString(36).substring(7),
      };
      this._events.push(eventToAdd);
      const addedEvent = await simulateLatency(eventToAdd);
      return ok(addedEvent);
    } catch (e) {
      return err({
        message: 'Failed to add event to mock service.',
        details: e as Error,
      });
    }
  }

  async update(updatedItem: Event): Promise<Result<Event, ApiError>> {
    try {
      const index = this._events.findIndex(
        (event) => event.id === updatedItem.id,
      );
      if (index !== -1) {
        this._events[index] = updatedItem;
        const updatedEvent = await simulateLatency(updatedItem);
        return ok(updatedEvent);
      }
      return err({ message: 'Event not found', statusCode: 404 });
    } catch (e) {
      return err({
        message: 'Failed to update event in mock service.',
        details: e as Error,
      });
    }
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    try {
      const initialLength = this._events.length;
      this._events = this._events.filter((event) => event.id !== id);
      if (this._events.length === initialLength) {
        return err({ message: 'Event not found', statusCode: 404 });
      }
      await simulateLatency(undefined);
      return ok(undefined);
    } catch (e) {
      return err({
        message: 'Failed to delete event from mock service.',
        details: e as Error,
      });
    }
  }

  async loadItems(
    filters: EventFilter,
    page: number = 1,
    itemsPerPage: number = 10,
  ): Promise<Result<Paginated<Event>, ApiError>> {
    try {
      let filteredEvents = this.events;

      if (filters.searchQuery) {
        const lowerCaseSearchQuery = filters.searchQuery.toLowerCase();
        filteredEvents = filteredEvents.filter(
          (event) =>
            event.name.toLowerCase().includes(lowerCaseSearchQuery) ||
            (event.description &&
              event.description.toLowerCase().includes(lowerCaseSearchQuery)),
        );
      }

      if (filters.type) {
        filteredEvents = filteredEvents.filter(
          (event) => event.type === filters.type,
        );
      }

      if (filters.familyId) {
        filteredEvents = filteredEvents.filter(
          (event) => event.familyId === filters.familyId,
        );
      }

      if (filters.relatedMemberId) {
        filteredEvents = filteredEvents.filter((event) =>
          event.relatedMembers?.includes(filters.relatedMemberId!),
        );
      }

      if (filters.startDate) {
        filteredEvents = filteredEvents.filter(
          (event) =>
            event.startDate && new Date(event.startDate) >= filters.startDate!,
        );
      }

      if (filters.endDate) {
        filteredEvents = filteredEvents.filter(
          (event) =>
            event.startDate && new Date(event.startDate) <= filters.endDate!,
        );
      }

      if (filters.location) {
        const lowerCaseLocation = filters.location.toLowerCase();
        filteredEvents = filteredEvents.filter(
          (event) =>
            event.location &&
            event.location.toLowerCase().includes(lowerCaseLocation),
        );
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
      return err({
        message: 'Failed to search events from mock service.',
        details: e as Error,
      });
    }
  }

  async getByIds(ids: string[]): Promise<Result<Event[], ApiError>> {
    try {
      const events = await simulateLatency(
        this.events.filter((e) => ids.includes(e.id)),
      );
      return ok(events);
    } catch (e) {
      return err({
        message: 'Failed to get events by IDs from mock service.',
        details: e as Error,
      });
    }
  }
}
