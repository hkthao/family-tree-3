// tests/unit/composables/event/logic/useEventSearch.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventSearch } from '@/composables/event/logic/useEventSearch';
import type { EventFilter } from '@/types';
import { EventType } from '@/types';
import { CalendarType } from '@/types/enums';

// Mock useI18n
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(() => ({
    t: vi.fn((key) => key), // Mock t function to return the key itself
  })),
}));

describe('useEventSearch', () => {
  let emit: vi.Mock;
  let composable: ReturnType<typeof useEventSearch>;
  let mockT: vi.Mock;

  beforeEach(() => {
    vi.clearAllMocks();
    emit = vi.fn();
    mockT = vi.mocked(useI18n().t);
    composable = useEventSearch(emit);
  });

  it('should initialize with default state', () => {
    expect(composable.state.expanded.value).toBe(false);
    expect(composable.state.filters.value).toEqual({
      type: undefined,
      memberId: null,
      startDate: undefined,
      endDate: undefined,
      calendarType: undefined,
    });
  });

  it('should return correct eventTypes', () => {
    const eventTypes = composable.state.eventTypes.value;
    expect(eventTypes).toHaveLength(4);
    expect(eventTypes[0]).toEqual({ title: 'event.type.birth', value: EventType.Birth });
    expect(eventTypes[1]).toEqual({ title: 'event.type.marriage', value: EventType.Marriage });
    expect(eventTypes[2]).toEqual({ title: 'event.type.death', value: EventType.Death });
    expect(eventTypes[3]).toEqual({ title: 'event.type.other', value: EventType.Other });
    expect(mockT).toHaveBeenCalledWith('event.type.birth');
  });

  it('should return correct calendarTypes', () => {
    const calendarTypes = composable.state.calendarTypes.value;
    expect(calendarTypes).toHaveLength(2);
    expect(calendarTypes[0]).toEqual({ title: 'event.calendarType.solar', value: CalendarType.Solar });
    expect(calendarTypes[1]).toEqual({ title: 'event.calendarType.lunar', value: CalendarType.Lunar });
    expect(mockT).toHaveBeenCalledWith('event.calendarType.solar');
  });

  it('should emit update:filters when applyFilters is called', () => {
    const testFilters: EventFilter = { type: EventType.Birth, familyId: '123' };
    composable.state.filters.value = testFilters;
    composable.actions.applyFilters();
    expect(emit).toHaveBeenCalledWith('update:filters', testFilters);
  });

  it('should reset filters and emit update:filters when resetFilters is called', () => {
    composable.state.filters.value.type = EventType.Death;
    composable.actions.resetFilters();
    expect(composable.state.filters.value).toEqual({
      type: undefined,
      memberId: null,
      startDate: undefined,
      endDate: undefined,
      calendarType: undefined,
    });
    expect(emit).toHaveBeenCalledWith('update:filters', {
      type: undefined,
      memberId: null,
      startDate: undefined,
      endDate: undefined,
      calendarType: undefined,
    });
  });

  it('should call applyFilters on filter change', async () => {
    const newStartDate = new Date('2023-01-01');
    composable.state.filters.value.startDate = newStartDate;
    await nextTick(); // Wait for watcher to trigger
    expect(emit).toHaveBeenCalledWith('update:filters', expect.objectContaining({ startDate: newStartDate }));
  });
});
