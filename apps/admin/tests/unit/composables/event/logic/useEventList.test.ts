// tests/unit/composables/event/logic/useEventList.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref, onMounted, watch } from 'vue';
import { useEventList } from '@/composables/event/logic/useEventList';
import type { EventFilter, Paginated, ListOptions } from '@/types';
import type { UseConfirmDialogReturn } from '@/composables/common/useConfirmDialog';
import type { UseGlobalSnackbarReturn } from '@/composables/common/useGlobalSnackbar';
import type { UseCrudDrawerReturn } from '@/composables/common/useCrudDrawer';
import type { UseEventListFiltersReturn } from '@/composables/event/logic/useEventListFilters';
import type { UseEventsQueryReturn } from '@/composables/event/queries/useEventsQuery';
import type { UseDeleteEventMutationReturn } from '@/composables/event/mutations/useDeleteEventMutation';
import type { Event } from '@/types';

// Mock Vue hooks and composables
vi.mock('vue', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue')>();
  return {
    ...actual,
    onMounted: vi.fn(),
    watch: vi.fn(),
  };
});
vi.mock('vue-i18n', () => ({
  useI18n: () => ({ t: vi.fn((key) => key) }),
}));
vi.mock('@/composables', () => ({
  useConfirmDialog: vi.fn(),
  useGlobalSnackbar: vi.fn(),
  useCrudDrawer: vi.fn(),
}));
vi.mock('@/composables/event/logic/useEventListFilters', () => ({
  useEventListFilters: vi.fn(),
}));
vi.mock('@/composables/event/queries/useEventsQuery', () => ({
  useEventsQuery: vi.fn(),
}));
vi.mock('@/composables/event/mutations/useDeleteEventMutation', () => ({
  useDeleteEventMutation: vi.fn(),
}));

describe('useEventList', () => {
  let mockT: vi.Mock;
  let mockShowConfirmDialog: vi.Mock;
  let mockShowSnackbar: vi.Mock;
  let mockCrudDrawer: UseCrudDrawerReturn<string>;
  let mockEventListFilters: UseEventListFiltersReturn;
  let mockEventsQuery: UseEventsQueryReturn;
  let mockDeleteEventMutation: UseDeleteEventMutationReturn;

  const mockFamilyId = 'family123';
  const mockEvent: Event = {
    id: 'event1',
    name: 'Test Event',
    familyId: mockFamilyId,
    type: 'Other',
    calendarType: 'Solar',
    solarDate: new Date(),
    repeatRule: 'None',
    description: '',
    relatedMemberIds: [],
    color: '#000000',
  };

  beforeEach(() => {
    vi.clearAllMocks();

    mockT = vi.fn((key) => key);
    vi.mocked(useI18n).mockReturnValue({ t: mockT } as any);

    mockShowConfirmDialog = vi.fn();
    vi.mocked(useConfirmDialog).mockReturnValue({ showConfirmDialog: mockShowConfirmDialog } as UseConfirmDialogReturn);

    mockShowSnackbar = vi.fn();
    vi.mocked(useGlobalSnackbar).mockReturnValue({ showSnackbar: mockShowSnackbar } as UseGlobalSnackbarReturn);

    mockCrudDrawer = {
      addDrawer: ref(false),
      editDrawer: ref(false),
      detailDrawer: ref(false),
      selectedItemId: ref(null),
      openAddDrawer: vi.fn(),
      openEditDrawer: vi.fn(),
      openDetailDrawer: vi.fn(),
      closeAddDrawer: vi.fn(),
      closeEditDrawer: vi.fn(),
      closeDetailDrawer: vi.fn(),
      closeAllDrawers: vi.fn(),
    };
    vi.mocked(useCrudDrawer).mockReturnValue(mockCrudDrawer);

    mockEventListFilters = {
      searchQuery: ref(''),
      filters: ref({ familyId: mockFamilyId }),
      page: ref(1),
      itemsPerPage: ref(10),
      sortBy: ref([]),
      setPage: vi.fn(),
      setItemsPerPage: vi.fn(),
      setSortBy: vi.fn(),
      setSearchQuery: vi.fn(),
      setFilters: vi.fn(),
    };
    vi.mocked(useEventListFilters).mockReturnValue(mockEventListFilters);

    mockEventsQuery = {
      events: ref([mockEvent]),
      totalItems: ref(1),
      loading: ref(false),
      refetch: vi.fn(),
      error: ref(null),
      query: { isError: ref(false) } as any,
    };
    vi.mocked(useEventsQuery).mockReturnValue(mockEventsQuery);

    mockDeleteEventMutation = {
      mutate: vi.fn(),
      isPending: ref(false),
    };
    vi.mocked(useDeleteEventMutation).mockReturnValue(mockDeleteEventMutation);
  });

  it('should initialize filters on mounted and watch familyId changes', () => {
    const { state } = useEventList({ familyId: mockFamilyId }, vi.fn());

    expect(onMounted).toHaveBeenCalledTimes(1);
    // Directly call the onMounted callback
    vi.mocked(onMounted).mock.calls[0][0]();
    expect(mockEventListFilters.setFilters).toHaveBeenCalledWith({ familyId: mockFamilyId });

    // Simulate watch callback
    expect(watch).toHaveBeenCalledTimes(1);
    const watchCallback = vi.mocked(watch).mock.calls[0][1];
    watchCallback(mockFamilyId + '_new');
    expect(mockEventListFilters.setFilters).toHaveBeenCalledWith({ familyId: mockFamilyId + '_new' });
  });

  it('should call setFilters for handleFilterUpdate', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    const newFilters: Omit<EventFilter, 'searchQuery'> = { type: 'Birth' };
    actions.handleFilterUpdate(newFilters);
    expect(mockEventListFilters.setFilters).toHaveBeenCalledWith(newFilters);
  });

  it('should call setSearchQuery for handleSearchUpdate', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.handleSearchUpdate('new search');
    expect(mockEventListFilters.setSearchQuery).toHaveBeenCalledWith('new search');
  });

  it('should call setPage, setItemsPerPage, setSortBy for handleListOptionsUpdate', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    const options = {
      page: 2,
      itemsPerPage: 25,
      sortBy: [{ key: 'date', order: 'desc' as const }],
    };
    actions.handleListOptionsUpdate(options);
    expect(mockEventListFilters.setPage).toHaveBeenCalledWith(2);
    expect(mockEventListFilters.setItemsPerPage).toHaveBeenCalledWith(25);
    expect(mockEventListFilters.setSortBy).toHaveBeenCalledWith([{ key: 'date', order: 'desc' }]);
  });

  describe('confirmDelete', () => {
    it('should delete event and show success snackbar if confirmed', async () => {
      mockShowConfirmDialog.mockResolvedValue(true);
      mockDeleteEventMutation.mutate.mockImplementation((_, callbacks) => callbacks.onSuccess());

      const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
      await actions.confirmDelete(mockEvent.id, mockEvent.name);

      expect(mockShowConfirmDialog).toHaveBeenCalledWith(expect.any(Object));
      expect(mockDeleteEventMutation.mutate).toHaveBeenCalledWith(mockEvent.id, expect.any(Object));
      expect(mockShowSnackbar).toHaveBeenCalledWith('event.messages.deleteSuccess', 'success');
      expect(mockEventsQuery.refetch).toHaveBeenCalled();
    });

    it('should show error snackbar if delete fails', async () => {
      mockShowConfirmDialog.mockResolvedValue(true);
      const errorMessage = 'Delete failed';
      mockDeleteEventMutation.mutate.mockImplementation((_, callbacks) => callbacks.onError(new Error(errorMessage)));

      const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
      await actions.confirmDelete(mockEvent.id, mockEvent.name);

      expect(mockShowConfirmDialog).toHaveBeenCalledWith(expect.any(Object));
      expect(mockDeleteEventMutation.mutate).toHaveBeenCalledWith(mockEvent.id, expect.any(Object));
      expect(mockShowSnackbar).toHaveBeenCalledWith(errorMessage, 'error');
      expect(mockEventsQuery.refetch).not.toHaveBeenCalled(); // Refetch only on success
    });

    it('should not delete event if not confirmed', async () => {
      mockShowConfirmDialog.mockResolvedValue(false);

      const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
      await actions.confirmDelete(mockEvent.id, mockEvent.name);

      expect(mockShowConfirmDialog).toHaveBeenCalledWith(expect.any(Object));
      expect(mockDeleteEventMutation.mutate).not.toHaveBeenCalled();
      expect(mockShowSnackbar).not.toHaveBeenCalled();
      expect(mockEventsQuery.refetch).not.toHaveBeenCalled();
    });
  });

  it('should close all drawers and refetch on handleEventSaved', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.handleEventSaved();
    expect(mockCrudDrawer.closeAllDrawers).toHaveBeenCalledTimes(1);
    expect(mockEventsQuery.refetch).toHaveBeenCalledTimes(1);
  });

  it('should call openAddDrawer', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.openAddDrawer();
    expect(mockCrudDrawer.openAddDrawer).toHaveBeenCalledTimes(1);
  });

  it('should call openEditDrawer', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.openEditDrawer('test-id');
    expect(mockCrudDrawer.openEditDrawer).toHaveBeenCalledWith('test-id');
  });

  it('should call openDetailDrawer', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.openDetailDrawer('test-id');
    expect(mockCrudDrawer.openDetailDrawer).toHaveBeenCalledWith('test-id');
  });

  it('should call closeAddDrawer', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.closeAddDrawer();
    expect(mockCrudDrawer.closeAddDrawer).toHaveBeenCalledTimes(1);
  });

  it('should call closeEditDrawer', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.closeEditDrawer();
    expect(mockCrudDrawer.closeEditDrawer).toHaveBeenCalledTimes(1);
  });

  it('should call closeDetailDrawer', () => {
    const { actions } = useEventList({ familyId: mockFamilyId }, vi.fn());
    actions.closeDetailDrawer();
    expect(mockCrudDrawer.closeDetailDrawer).toHaveBeenCalledTimes(1);
  });
});
