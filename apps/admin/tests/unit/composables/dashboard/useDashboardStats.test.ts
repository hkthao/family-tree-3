import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useDashboardStats } from '@/composables/dashboard/useDashboardStats';
import { ref, type Ref, computed,  unref, nextTick } from 'vue'; // Import unref
import type { ApiError, DashboardStats } from '@/types';
import type { IDashboardService } from '@/services/dashboard/dashboard.service.interface';
import { useQuery } from '@tanstack/vue-query'; // Import the mocked useQuery

// Mock the external dependencies
vi.mock('@tanstack/vue-query', () => {

  const mockUseQuery = vi.fn((options) => {
    const queryResultData: Ref<any> = ref(options?.initialData || options?.placeholderData);
    const queryResultLoading: Ref<boolean> = ref(options?.enabled !== false);
    const queryResultError: Ref<any> = ref(null);

    const executeQuery = async () => {
      queryResultLoading.value = true;
      try {
        const data = await options.queryFn();
        queryResultData.value = data;
        queryResultError.value = null;
      } catch (err) {
        queryResultError.value = err;
        queryResultData.value = undefined;
      } finally {
        queryResultLoading.value = false;
      }
    };

    const refetch = vi.fn(async () => {
        await executeQuery();
        return { data: queryResultData.value };
    });

    const queryResult = {
      data: queryResultData,
      isLoading: queryResultLoading,
      isError: computed(() => !!queryResultError.value),
      error: queryResultError,
      isFetching: queryResultLoading,
      refetch: refetch,
    };

    Object.defineProperty(queryResult, 'enabled', {
      get() {
        return (options && typeof options.enabled !== 'undefined') ? unref(options.enabled) : true;
      },
    });

    return queryResult;
  });

  const mockUseMutation = vi.fn((options) => {
      const isPending = ref(false);
      const error = ref(null);
      const mutate = vi.fn(async (variables, callbacks) => {
          isPending.value = true;
          try {
              const data = await options.mutationFn(variables);
              isPending.value = false; // Set pending to false before calling onSuccess
              callbacks?.onSuccess?.(data, variables, null);
              return data;
          } catch (err) {
              error.value = err;
              isPending.value = false; // Set pending to false on error as well
              callbacks?.onError?.(err, variables, null);
              throw err;
          }
      });
      return {
          mutate,
          isPending,
          error,
      };
  });

  const mockUseQueryClient = vi.fn(() => ({
    invalidateQueries: vi.fn(),
    setQueryData: vi.fn(),
    getQueryData: vi.fn(),
  }));

  return {
    useQuery: mockUseQuery,
    useMutation: mockUseMutation,
    useQueryClient: mockUseQueryClient,
  };
});

// Mock dashboardService
const mockDashboardService: IDashboardService = {
  fetchStats: vi.fn(),
};

describe('useDashboardStats', () => {
  const mockDashboardStats: DashboardStats = {
    totalFamilies: 1,
    totalMembers: 10,
    totalEvents: 5,
    averageMemberAge: 45,
    membersLiving: 8,
    membersDeceased: 2,
    genderRatio: { male: 5, female: 5, unknown: 0 },
    birthsPerMonth: [],
    deathsPerMonth: [],
  };

  beforeEach(() => {
    vi.clearAllMocks();
    // No need to reset mockUseQuery.mockImplementation here, it's done once by vi.mock
  });

  it('should initialize with correct data and status', async () => {
    (mockDashboardService.fetchStats as vi.Mock).mockResolvedValueOnce({ ok: true, value: mockDashboardStats });
    
    const familyIdRef = ref('family1');
    const { state, actions } = useDashboardStats(familyIdRef, {
        useQuery: useQuery as any, // Pass the mocked useQuery
        getDashboardService: () => mockDashboardService,
    });

    expect(state.isLoading.value).toBe(true); // Should be loading initially
    await actions.refetch(); // Manually trigger the fetch

    expect(state.isLoading.value).toBe(false);
    expect(state.dashboardStats.value).toEqual(mockDashboardStats);
    expect(mockDashboardService.fetchStats).toHaveBeenCalledWith('family1');
  });

  it('should refetch when familyId changes', async () => {
    const familyIdRef = ref('family1');
    (mockDashboardService.fetchStats as vi.Mock).mockResolvedValueOnce({ ok: true, value: mockDashboardStats });
    
    const { actions } = useDashboardStats(familyIdRef, {
        useQuery: useQuery as any, // Pass the mocked useQuery
        getDashboardService: () => mockDashboardService,
    });

    // Initial fetch
    await actions.refetch();
    expect(mockDashboardService.fetchStats).toHaveBeenCalledWith('family1');
    mockDashboardService.fetchStats.mockClear();

    familyIdRef.value = 'family2';
    // Await reactivity and then refetch triggered by the watch effect in the composable
    await nextTick();
    await actions.refetch(); // The watch effect should trigger a refetch, but we explicitly await it here to be sure

    expect(mockDashboardService.fetchStats).toHaveBeenCalledWith('family2');
  });

  it('should handle error during fetchStats', async () => {
    const familyIdRef = ref('family1');
    const mockError: ApiError = { message: 'Failed to fetch stats', statusCode: 500 };
    (mockDashboardService.fetchStats as vi.Mock).mockResolvedValueOnce({ ok: false, error: mockError });
    
    const { state, actions } = useDashboardStats(familyIdRef, {
        useQuery: useQuery as any, // Pass the mocked useQuery
        getDashboardService: () => mockDashboardService,
    });

    await actions.refetch(); // Trigger the fetch

    expect(state.isLoading.value).toBe(false);
    expect(state.error.value).toEqual(mockError);
    expect(state.dashboardStats.value).toBeUndefined();
  });
});