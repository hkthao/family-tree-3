// tests/unit/composables/event/logic/eventTimeline.logic.test.ts
import { describe, it, expect, vi } from 'vitest';
import {
  mapFiltersToQueryOptions,
  sortEventsBySolarDateDesc,
} from '@/composables/event/logic/eventTimeline.logic';
import type { Event, EventFilter, Paginated, ListOptions } from '@/types';
import { cloneDeep } from 'lodash';

// Mock lodash.cloneDeep
vi.mock('lodash', () => ({
  cloneDeep: vi.fn((value) => JSON.parse(JSON.stringify(value))),
}));

describe('eventTimeline.logic', () => {
  describe('mapFiltersToQueryOptions', () => {
    it('should correctly map EventFilter to ListOptions and FilterOptions', () => {
      const filters: EventFilter = {
        searchQuery: 'test',
        familyId: 'fam1',
        type: 'Birth',
        page: 2,
        itemsPerPage: 20,
        sortBy: [{ key: 'name', order: 'asc' }],
      };
      const currentPage = 3;
      const itemsPerPage = 15;
      const sortBy: ListOptions['sortBy'] = [{ key: 'date', order: 'desc' }];

      const { listOptions, filterOptions } = mapFiltersToQueryOptions(
        filters,
        currentPage,
        itemsPerPage,
        sortBy
      );

      expect(vi.mocked(cloneDeep)).toHaveBeenCalledWith(filters);
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
        lunarStartDay: filters.lunarStartDay,
        lunarStartMonth: filters.lunarStartMonth,
        lunarEndDay: filters.lunarEndDay,
        lunarEndMonth: filters.lunarEndMonth,
      });
    });
  });

  describe('sortEventsBySolarDateDesc', () => {
    it('should sort events by solarDate in descending order', () => {
      const event1: Event = { id: '1', solarDate: new Date('2024-01-01'), familyId: 'f1', name: 'e1', type: 'Other', calendarType: 'Solar', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000' };
      const event2: Event = { id: '2', solarDate: new Date('2024-01-03'), familyId: 'f1', name: 'e2', type: 'Other', calendarType: 'Solar', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000' };
      const event3: Event = { id: '3', solarDate: new Date('2024-01-02'), familyId: 'f1', name: 'e3', type: 'Other', calendarType: 'Solar', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000' };
      const eventWithoutDate: Event = { id: '4', solarDate: null, familyId: 'f1', name: 'e4', type: 'Other', calendarType: 'Solar', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000' };

      const paginatedData: Paginated<Event> = {
        items: [event1, event2, event3, eventWithoutDate],
        totalItems: 4,
        totalPages: 1,
        pageNumber: 1,
        pageSize: 10,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      const sorted = sortEventsBySolarDateDesc(paginatedData);
      expect(sorted.items).toEqual([event2, event3, event1, eventWithoutDate]);
    });

    it('should handle empty items array', () => {
      const paginatedData: Paginated<Event> = {
        items: [],
        totalItems: 0,
        totalPages: 0,
        pageNumber: 1,
        pageSize: 10,
        hasPreviousPage: false,
        hasNextPage: false,
      };
      const sorted = sortEventsBySolarDateDesc(paginatedData);
      expect(sorted.items).toEqual([]);
    });
  });
});
