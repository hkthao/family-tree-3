import mockEvents from '@/data/mock/events.json';
import { simulateLatency } from '@/utils/mockUtils';
import type { Result, Event, EventFilter, Paginated } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { IEventService } from './event.service.interface';

export class MockEventService implements IEventService {
  private events: Event[] = [...mockEvents as unknown as Event[]];

  async fetch(): Promise<Result<Event[], ApiError>> {
    try {
      const events = await simulateLatency(this.events);
      return ok(events);
    } catch (e) {
      return err({
        name: 'ApiError',
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
        name: 'ApiError',
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
      this.events.push(eventToAdd);
      const addedEvent = await simulateLatency(eventToAdd);
      return ok(addedEvent);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to add event to mock service.',
        details: e as Error,
      });
    }
  }

  async update(updatedItem: Event): Promise<Result<Event, ApiError>> {
    try {
      const index = this.events.findIndex(
        (event) => event.id === updatedItem.id,
      );
      if (index !== -1) {
        this.events[index] = updatedItem;
        const updatedEvent = await simulateLatency(updatedItem);
                  return ok(updatedEvent);
                }
                return err({ name: 'ApiError', message: 'Event not found', statusCode: 404 });    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to update event in mock service.',
        details: e as Error,
      });
    }
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    try {
      const initialLength = this.events.length;
      this.events = this.events.filter((event) => event.id !== id);
      if (this.events.length === initialLength) {
        return err({ name: 'ApiError', message: 'Event not found', statusCode: 404 });
      }
      await simulateLatency(undefined);
      return ok(undefined);
    } catch (e) {
      return err({
        name: 'ApiError',
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
        name: 'ApiError',
        message: 'Failed to search events from mock service.',
        details: e as Error,
      });
    }
  }

  async getByIds(ids: string[]): Promise<Result<Event[], ApiError>> {
    try {
      const events = await simulateLatency(
        this.events.filter((e) => e.id !== undefined && ids.includes(e.id as string)),
      );
      return ok(events);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to get events by IDs from mock service.',
        details: e as Error,
      });
    }
  }

  async getUpcomingEvents(familyId?: string): Promise<Result<Event[], ApiError>> {
    try {
      const today = new Date();
      const thirtyDaysFromNow = new Date();
      thirtyDaysFromNow.setDate(today.getDate() + 30);

      let upcoming = this.events.filter(event => {
        const eventStartDate = event.startDate ? new Date(event.startDate) : null;
        return eventStartDate && eventStartDate >= today && eventStartDate <= thirtyDaysFromNow;
      });

      if (familyId) {
        upcoming = upcoming.filter(event => event.familyId === familyId);
      }

      upcoming.sort((a, b) => {
        const dateA = a.startDate ? new Date(a.startDate).getTime() : 0;
        const dateB = b.startDate ? new Date(b.startDate).getTime() : 0;
        return dateA - dateB;
      });

      const result = await simulateLatency(upcoming);
      return ok(result);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to fetch upcoming events from mock service.',
        details: e as Error,
      });
    }
  }

  async addItems(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>> {
    try {
      const newIds: string[] = [];
      newItems.forEach(newItem => {
        const eventToAdd = {
          ...newItem,
          id: 'mock-id-' + Math.random().toString(36).substring(7),
        };
        this.events.push(eventToAdd);
        newIds.push(eventToAdd.id);
      });
      await simulateLatency(undefined);
      return ok(newIds);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to add multiple events to mock service.',
        details: e as Error,
      });
    }
  }
}
