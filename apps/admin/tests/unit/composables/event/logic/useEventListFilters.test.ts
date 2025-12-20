// tests/unit/composables/event/logic/useEventListFilters.test.ts
import { describe, it, expect, beforeEach } from 'vitest';
import { useEventListFilters } from '@/composables/event/logic/useEventListFilters';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { EventFilter } from '@/types';

describe('useEventListFilters', () => {
  let composable: ReturnType<typeof useEventListFilters>;

  beforeEach(() => {
    composable = useEventListFilters();
  });

  it('should initialize with default filters', () => {
    expect(composable.searchQuery.value).toBe('');
    expect(composable.page.value).toBe(1);
    expect(composable.itemsPerPage.value).toBe(DEFAULT_ITEMS_PER_PAGE);
    expect(composable.sortBy.value).toEqual([]);
    expect(composable.filters.value).toEqual({
      searchQuery: '',
      page: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      sortBy: [],
    });
  });

  it('should update page correctly with setPage', () => {
    composable.setPage(2);
    expect(composable.page.value).toBe(2);
    expect(composable.filters.value.page).toBe(2);
  });

  it('should update itemsPerPage and reset page with setItemsPerPage', () => {
    composable.setPage(3); // Set to a different page first
    composable.setItemsPerPage(25);
    expect(composable.itemsPerPage.value).toBe(25);
    expect(composable.page.value).toBe(1); // Page should reset
    expect(composable.filters.value.itemsPerPage).toBe(25);
    expect(composable.filters.value.page).toBe(1);
  });

  it('should update sortBy correctly with setSortBy', () => {
    const newSortBy = [{ key: 'name', order: 'asc' }];
    composable.setSortBy(newSortBy);
    expect(composable.sortBy.value).toEqual(newSortBy);
    expect(composable.filters.value.sortBy).toEqual(newSortBy);
  });

  it('should update searchQuery and reset page with setSearchQuery', () => {
    composable.setPage(3); // Set to a different page first
    composable.setSearchQuery('test query');
    expect(composable.searchQuery.value).toBe('test query');
    expect(composable.page.value).toBe(1); // Page should reset
    expect(composable.filters.value.searchQuery).toBe('test query');
    expect(composable.filters.value.page).toBe(1);
  });

  it('should merge new filters with setFilters and handle individual properties', () => {
    composable.setSearchQuery('initial');
    composable.setPage(2);
    composable.setItemsPerPage(20);
    composable.setSortBy([{ key: 'id', order: 'desc' }]);

    const newFilters: EventFilter = {
      searchQuery: 'updated',
      itemsPerPage: 30,
      familyId: 'family123',
      startDate: new Date('2024-01-01'),
    };

    composable.setFilters(newFilters);

    expect(composable.searchQuery.value).toBe('updated');
    expect(composable.itemsPerPage.value).toBe(30);
    expect(composable.filters.value.familyId).toBe('family123');
    expect(composable.filters.value.startDate).toEqual(newFilters.startDate);
    // Page and sortBy should remain from previous state if not explicitly set in newFilters
    expect(composable.page.value).toBe(2);
    expect(composable.sortBy.value).toEqual([{ key: 'id', order: 'desc' }]);

    // Test setting specific filter properties
    composable.setFilters({ page: 5 });
    expect(composable.page.value).toBe(5);

    composable.setFilters({ sortBy: [{ key: 'date', order: 'asc' }] });
    expect(composable.sortBy.value).toEqual([{ key: 'date', order: 'asc' }]);
  });
});
