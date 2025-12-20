// tests/unit/composables/event/logic/useEventListComposable.test.ts
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventListComposable } from '@/composables/event/logic/useEventListComposable';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import * as dateUtils from '@/utils/dateUtils'; // Import the module to mock formatDate

// Mock external dependencies
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(() => ({
    t: vi.fn((key) => key), // Mock t function to return the key itself
  })),
}));
vi.mock('@/utils/dateUtils', () => ({
  formatDate: vi.fn((date) => `formatted-${date}`), // Mock formatDate
}));

// Mock Vue's watch to immediately execute the callback for testing purposes if needed
// For debounced search, we need to control timers, so we'll use vi.useFakeTimers
vi.mock('vue', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue')>();
  return {
    ...actual,
    watch: vi.fn((source, cb, options) => {
      // For immediate watchers, call cb
      if (options?.immediate) {
        cb(actual.unref(source));
      }
      // Return a mock unwatch function
      return vi.fn();
    }),
    ref: actual.ref,
    computed: actual.computed,
  };
});

describe('useEventListComposable', () => {
  let emit: vi.Mock;
  let mockT: vi.Mock;
  let mockFormatDate: vi.Mock;
  const mockEvents = ref([]);
  const mockTotalEvents = ref(0);
  const mockLoading = ref(false);

  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers(); // Enable fake timers for debounced search

    emit = vi.fn();
    mockT = vi.mocked(useI18n().t);
    mockFormatDate = vi.mocked(dateUtils.formatDate);

    // Reset watch mock
    vi.mocked(watch).mockClear();
    vi.mocked(watch).mockImplementation((source, cb) => {
      // Simulate immediate execution for the initial watch call, but allow manual triggering for props.search
      let lastValue = source.value;
      Object.defineProperty(source, 'value', {
        get: () => lastValue,
        set: (newValue) => {
          if (newValue !== lastValue) {
            lastValue = newValue;
            cb(newValue, lastValue);
          }
        },
      });
      return vi.fn();
    });
  });

  afterEach(() => {
    vi.useRealTimers(); // Restore real timers
  });

  it('should initialize with default state', () => {
    const { state } = useEventListComposable(
      { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
      emit
    );
    expect(state.debouncedSearch.value).toBe('');
    expect(state.itemsPerPage.value).toBe(DEFAULT_ITEMS_PER_PAGE);
  });

  it('should compute headers correctly using i18n', () => {
    const { state } = useEventListComposable(
      { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
      emit
    );
    const headers = state.headers.value;
    expect(headers).toHaveLength(5);
    expect(headers[0].title).toBe('event.list.headers.date');
    expect(headers[1].title).toBe('event.list.headers.name');
    expect(mockT).toHaveBeenCalledWith('event.list.headers.date');
    expect(mockT).toHaveBeenCalledWith('event.list.headers.name');
  });

  describe('debouncedSearch', () => {
    it('should update searchQuery immediately', () => {
      const { state } = useEventListComposable(
        { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
        emit
      );
      state.debouncedSearch.value = 'new search value';
      expect(state.debouncedSearch.value).toBe('new search value');
      expect(emit).not.toHaveBeenCalled(); // Not emitted immediately
    });

    it('should debounce emit("update:search")', async () => {
      const { state } = useEventListComposable(
        { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
        emit
      );

      state.debouncedSearch.value = 'first';
      vi.advanceTimersByTime(100);
      state.debouncedSearch.value = 'second';
      vi.advanceTimersByTime(100);
      state.debouncedSearch.value = 'final search';

      expect(emit).not.toHaveBeenCalled();

      vi.advanceTimersByTime(300); // Advance past debounce time

      expect(emit).toHaveBeenCalledTimes(1);
      expect(emit).toHaveBeenCalledWith('update:search', 'final search');
    });
  });

  it('should update searchQuery when props.search changes', async () => {
    const props = ref({ events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: 'initial' });
    const { state } = useEventListComposable(
      props.value,
      emit
    );

    // Manually trigger the watch callback
    const watchCallback = vi.mocked(watch).mock.calls[0][1];
    watchCallback('new prop search');
    
    expect(state.debouncedSearch.value).toBe('new prop search');
  });

  it('should emit "update:options" for loadEvents', () => {
    const { actions } = useEventListComposable(
      { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
      emit
    );
    const options = { page: 1, itemsPerPage: 10, sortBy: [{ key: 'name', order: 'asc' as const }] };
    actions.loadEvents(options);
    expect(emit).toHaveBeenCalledWith('update:options', options);
  });

  it('should emit "edit" for editEvent', () => {
    const { actions } = useEventListComposable(
      { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
      emit
    );
    actions.editEvent('event1');
    expect(emit).toHaveBeenCalledWith('edit', 'event1');
  });

  it('should emit "delete" for confirmDelete', () => {
    const { actions } = useEventListComposable(
      { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
      emit
    );
    actions.confirmDelete('event1');
    expect(emit).toHaveBeenCalledWith('delete', 'event1');
  });

  it('should expose formatDate utility', () => {
    const { actions } = useEventListComposable(
      { events: mockEvents.value, totalEvents: mockTotalEvents.value, loading: mockLoading.value, search: '' },
      emit
    );
    const testDate = new Date();
    actions.formatDate(testDate);
    expect(mockFormatDate).toHaveBeenCalledWith(testDate);
  });
});
