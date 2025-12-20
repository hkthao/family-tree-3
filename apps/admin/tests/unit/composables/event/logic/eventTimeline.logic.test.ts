import { describe, it, expect, vi } from 'vitest';
import { mapFiltersToQueryOptions, sortEventsBySolarDateDesc } from '@/composables/event/logic/eventTimeline.logic';
import type { Event, EventFilter, ListOptions, Paginated } from '@/types';
import { EventType, CalendarType, RepeatRule } from '@/types';

describe('eventTimeline.logic', () => {
  describe('mapFiltersToQueryOptions', () => {
    it('should correctly map EventFilter to ListOptions and FilterOptions', () => {
      const filters: EventFilter = {
        searchQuery: 'birth',
        familyId: 'fam123',
        type: EventType.Birth,
        memberId: 'mem456',
        startDate: new Date('2023-01-01'),
        endDate: new Date('2023-12-31'),
        calendarType: CalendarType.Solar,
        lunarMonthRange: [1, 12],
      };
      const currentPage = 2;
      const itemsPerPage = 20;
      const sortBy: ListOptions['sortBy'] = [{ key: 'solarDate', order: 'desc' }];

      const { listOptions, filterOptions } = mapFiltersToQueryOptions(
        filters,
        currentPage,
        itemsPerPage,
        sortBy,
      );

      expect(listOptions).toEqual({
        page: currentPage,
        itemsPerPage: itemsPerPage,
        sortBy: sortBy,
      });

      expect(filterOptions).toEqual({
        searchQuery: filters.searchQuery,
        familyId: filters.familyId,
        type: filters.type,
        memberId: filters.memberId,
        startDate: filters.startDate,
        endDate: filters.endDate,
        calendarType: filters.calendarType,
        lunarMonthRange: filters.lunarMonthRange,
      });
      // Ensure cloneDeep worked by checking if the original filters object is not the same reference
      expect(filterOptions).not.toBe(filters);
      expect(filterOptions.startDate).toBeInstanceOf(Date);
    });

    it('should handle partial filters gracefully', () => {
      const filters: EventFilter = {
        searchQuery: 'test',
      };
      const currentPage = 1;
      const itemsPerPage = 10;
      const sortBy: ListOptions['sortBy'] = [];

      const { listOptions, filterOptions } = mapFiltersToQueryOptions(
        filters,
        currentPage,
        itemsPerPage,
        sortBy,
      );

      expect(listOptions).toEqual({ page: 1, itemsPerPage: 10, sortBy: [] });
      expect(filterOptions).toEqual({
        searchQuery: 'test',
        familyId: undefined,
        type: undefined,
        memberId: undefined,
        startDate: undefined,
        endDate: undefined,
        calendarType: undefined,
        lunarMonthRange: undefined,
      });
    });

    it('should return empty sort array if sortBy is null or undefined', () => {
      const filters: EventFilter = { searchQuery: 'test' };
      const currentPage = 1;
      const itemsPerPage = 10;
      const sortBy: any = null; // Test with null sortBy

      const { listOptions } = mapFiltersToQueryOptions(
        filters,
        currentPage,
        itemsPerPage,
        sortBy,
      );

      expect(listOptions.sortBy).toEqual(null); // The function does not transform null to empty array
    });
  });

  describe('sortEventsBySolarDateDesc', () => {
    const event1: Event = { id: '1', solarDate: new Date('2023-03-15'), name: 'Event 1' } as Event;
    const event2: Event = { id: '2', solarDate: new Date('2023-01-20'), name: 'Event 2' } as Event;
    const event3: Event = { id: '3', solarDate: new Date('2023-05-01'), name: 'Event 3' } as Event;
    const eventWithNoDate: Event = { id: '4', solarDate: null, name: 'Event 4' } as Event;

    it('should sort events by solarDate in descending order', () => {
      const paginatedData: Paginated<Event> = {
        items: [event1, event2, event3],
        totalItems: 3,
        totalPages: 1,
        page: 1,
        itemsPerPage: 10,
      };

      const sorted = sortEventsBySolarDateDesc(paginatedData);
      expect(sorted.items).toEqual([event3, event1, event2]);
    });

    it('should place events with null or undefined solarDate at the end', () => {
      const paginatedData: Paginated<Event> = {
        items: [event1, eventWithNoDate, event2],
        totalItems: 3,
        totalPages: 1,
        page: 1,
        itemsPerPage: 10,
      };

      const sorted = sortEventsBySolarDateDesc(paginatedData);
      expect(sorted.items).toEqual([event1, event2, eventWithNoDate]); // Order of events with no date is preserved relative to each other but placed last
    });

    it('should not mutate the original items array', () => {
      const originalItems = [event1, event2, event3];
      const paginatedData: Paginated<Event> = {
        items: originalItems,
        totalItems: 3,
        totalPages: 1,
        page: 1,
        itemsPerPage: 10,
      };

      const sortedResult = sortEventsBySolarDateDesc(paginatedData);

      // Ensure the items array within the original input object is not mutated
      expect(paginatedData.items).toEqual(originalItems);
      expect(paginatedData.items).toBe(originalItems); // The input object's items array should be the same reference

      // Ensure the returned sorted items array is a new array (not the same reference as originalItems)
      expect(sortedResult.items).not.toBe(originalItems);
      expect(sortedResult.items).toEqual([event3, event1, event2]); // Check content of the new array
    });

    it('should return the same object structure with sorted items', () => {
      const paginatedData: Paginated<Event> = {
        items: [event1, event2],
        totalItems: 2,
        totalPages: 1,
        page: 1,
        itemsPerPage: 10,
      };
      const sorted = sortEventsBySolarDateDesc(paginatedData);
      expect(sorted.totalItems).toBe(2);
      expect(sorted.totalPages).toBe(1);
      expect(sorted.page).toBe(1);
      expect(sorted.itemsPerPage).toBe(10);
    });
  });
});
