// src/composables/event/logic/eventTimeline.logic.ts
import type { EventDto, EventFilter, ListOptions, Paginated } from '@/types';
import { cloneDeep } from 'lodash';

/**
 * Maps EventFilter to ListOptions and FilterOptions for event service search.
 * @param filters The event filter object.
 * @param currentPage The current page number.
 * @param itemsPerPage The number of items per page.
 * @param sortBy The sort by options.
 * @returns An object containing ListOptions and FilterOptions.
 */
export function mapFiltersToQueryOptions(
  filters: EventFilter,
  currentPage: number,
  itemsPerPage: number,
  sortBy: ListOptions['sortBy']
): { listOptions: ListOptions; filterOptions: EventFilter } {
  const currentFilters = cloneDeep(filters);

  const listOptions: ListOptions = {
    page: currentPage,
    itemsPerPage: itemsPerPage,
    sortBy: sortBy,
  };

  const filterOptions: EventFilter = {
    searchQuery: currentFilters.searchQuery,
    familyId: currentFilters.familyId,
    type: currentFilters.type,
    memberId: currentFilters.memberId,
    startDate: currentFilters.startDate,
    endDate: currentFilters.endDate,
    calendarType: currentFilters.calendarType,
    lunarMonthRange: currentFilters.lunarMonthRange,
  };

  return { listOptions, filterOptions };
}

/**
 * Sorts a list of events by solarDate in descending order.
 * @param data The paginated event data.
 * @returns Paginated event data with items sorted.
 */
export function sortEventsBySolarDateDesc(data: Paginated<EventDto>): Paginated<EventDto> {
  const sortedItems = [...data.items].sort((a, b) => {
    // Events with null/undefined solarDate should come last
    if (!a.solarDate && !b.solarDate) return 0; // Both are null, preserve original order
    if (!a.solarDate) return 1; // a is null, b comes first
    if (!b.solarDate) return -1; // b is null, a comes first

    // Otherwise, sort by solarDate in descending order
    return new Date(b.solarDate).getTime() - new Date(a.solarDate).getTime();
  });
  return { ...data, items: sortedItems };
}
