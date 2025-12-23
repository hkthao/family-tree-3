// src/composables/event/event.adapter.ts
import { ApiEventService } from '@/services/event/api.event.service';
import type { IEventService } from '@/services/event/event.service.interface';
import { apiClient } from '@/plugins/axios';
import type { Event, Result } from '@/types'; // Import Result type

/**
 * @interface EventServiceAdapter
 * @description Defines the interface for an event service adapter,
 * ensuring consistency and testability across different event service implementations.
 */
export interface EventServiceAdapter extends IEventService {
  /**
   * Adds a new event.
   * @param eventData The event data to add.
   * @returns A Result object containing either the added Event or an ApiError.
   */
  add(eventData: Omit<Event, 'id'>): Promise<Result<Event>>;

  /**
   * Updates an existing event.
   * @param updatedEvent The event data to update.
   * @returns A Result object containing either the updated Event or an ApiError.
   */
  update(updatedEvent: Event): Promise<Result<Event>>;

  /**
   * Deletes an event by its ID.
   * @param eventId The ID of the event to delete.
   * @returns A Result object indicating success or an ApiError.
   */
  delete(eventId: string): Promise<Result<void>>;

  /**
   * Retrieves an event by its ID.
   * @param eventId The ID of the event to retrieve.
   * @returns A Result object containing either the Event or an ApiError.
   */
  getById(eventId: string): Promise<Result<Event | undefined>>;

  /**
   * Searches for events based on provided list and filter options.
   * @param listOptions Options for pagination and sorting.
   * @param filterOptions Options for filtering events.
   * @returns A Result object containing paginated events or an ApiError.
   */
  search(listOptions: any, filterOptions: any): Promise<Result<any>>; // TODO: Define proper types for listOptions and filterOptions

  /**
   * Fetches a list of all events for a specific family.
   * @param familyId The ID of the family.
   * @returns A promise that resolves to a Result object containing an array of Event objects or an ApiError.
   */
  getEventsByFamilyId(familyId: string): Promise<Result<Event[]>>;
}

/**
 * @class ApiEventServiceAdapter
 * @description Adapts the ApiEventService to the EventServiceAdapter interface,
 * using the actual API client for network requests.
 */
export class ApiEventServiceAdapter implements EventServiceAdapter {
  private apiEventService: IEventService;

  constructor(apiEventService: IEventService) {
    this.apiEventService = apiEventService;
  }

  async add(eventData: Omit<Event, 'id'>): Promise<Result<Event>> {
    return this.apiEventService.add(eventData);
  }

  async update(updatedEvent: Event): Promise<Result<Event>> {
    return this.apiEventService.update(updatedEvent);
  }

  async delete(eventId: string): Promise<Result<void>> {
    return this.apiEventService.delete(eventId);
  }

  async getById(eventId: string): Promise<Result<Event | undefined>> {
    return this.apiEventService.getById(eventId);
  }

  async search(listOptions: any, filterOptions: any): Promise<Result<any>> {
    return this.apiEventService.search(listOptions, filterOptions);
  }

  async getEventsByFamilyId(familyId: string): Promise<Result<Event[]>> {
    return this.apiEventService.getEventsByFamilyId(familyId);
  }

  async getByIds(ids: string[]): Promise<Result<Event[]>> {
    return this.apiEventService.getByIds(ids);
  }
}

/**
 * @constant DefaultEventServiceAdapter
 * @description Provides a default instance of the ApiEventServiceAdapter.
 */
export const DefaultEventServiceAdapter: EventServiceAdapter = new ApiEventServiceAdapter(
  new ApiEventService(apiClient),
);
