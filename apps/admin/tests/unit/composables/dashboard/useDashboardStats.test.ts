import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useDashboardStats } from '@/composables/dashboard/useDashboardStats';
import { useQuery } from '@tanstack/vue-query';
import { ref, type Ref, computed } from 'vue';
import type { ApiError, DashboardStats } from '@/types';
import type { IDashboardService } from '@/services/dashboard/dashboard.service.interface';
import { queryKeys } from '@/constants/queryKeys';

// Mock the external dependencies
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
}));

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
  });

      it('should call dashboardService.fetchStats with the correct familyId in queryFn', async () => {
        const familyIdRef = ref('family1');
        
        // Mock useQuery to capture the queryFn passed to it
        let capturedQueryFn: (() => Promise<DashboardStats | undefined>) | undefined;
        (useQuery as vi.Mock).mockImplementation((options) => {
          capturedQueryFn = options.queryFn;
          return {
            data: ref(mockDashboardStats),
            isLoading: ref(false),
            isError: ref(false),
            error: ref(null),
            isFetching: ref(false),
            refetch: vi.fn(),
          };
        });
  
        mockDashboardService.fetchStats.mockResolvedValue({ ok: true, value: mockDashboardStats });
  
        useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });
  
        // Now, manually execute the captured queryFn to trigger the fetchStats call
        await capturedQueryFn?.();
  
        expect(mockDashboardService.fetchStats).toHaveBeenCalledWith('family1');
      });
  it('should return dashboardStats data on successful query', async () => {
    const familyIdRef = ref('family1');
    (useQuery as vi.Mock).mockImplementation(() => {
      return {
        data: ref(mockDashboardStats),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });
    mockDashboardService.fetchStats.mockResolvedValue({ ok: true, value: mockDashboardStats });

    const { state } = useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });

    expect(state.dashboardStats.value).toEqual(mockDashboardStats);
  });

  it('should return isLoading true while fetching', () => {
    const familyIdRef = ref('family1');
    (useQuery as vi.Mock).mockImplementation(() => {
      return {
        data: ref(undefined),
        isLoading: ref(true),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(true),
        refetch: vi.fn(),
      };
    });

    const { state } = useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });

    expect(state.isLoading.value).toBe(true);
    expect(state.isFetching.value).toBe(true);
  });

  it('should return isError true and error on failed query', async () => {
    const familyIdRef = ref('family1');
    const mockError: ApiError = { message: 'Failed to fetch stats', statusCode: 500 };
    (useQuery as vi.Mock).mockImplementation((options) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError));
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(true),
        error: ref(mockError),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });
    mockDashboardService.fetchStats.mockResolvedValue({ ok: false, error: mockError });

    const { state } = useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });

    expect(state.isError.value).toBe(true);
    expect(state.error.value).toEqual(mockError);
  });

  it('should set enabled to false if familyId is undefined', () => {
    const familyIdRef = ref(undefined);
    let enabledComputed: Ref<boolean> | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      enabledComputed = options.enabled;
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });

    expect(enabledComputed?.value).toBe(false);
  });

  it('should set enabled to true if familyId is defined and service is available', () => {
    const familyIdRef = ref('family1');
    let enabledComputed: Ref<boolean> | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      enabledComputed = options.enabled;
      return {
        data: ref(mockDashboardStats),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });

    expect(enabledComputed?.value).toBe(true);
  });

  it('should call refetch when actions.refetch is called', () => {
    const familyIdRef = ref('family1');
    const mockRefetch = vi.fn();

    (useQuery as vi.Mock).mockImplementation(() => {
      return {
        data: ref(mockDashboardStats),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: mockRefetch,
      };
    });

    const { actions } = useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });
    actions.refetch();

    expect(mockRefetch).toHaveBeenCalled();
  });

  it('should have the correct queryKey', () => {
    const familyIdRef = ref('family1');
    let queryKeyComputed: Ref<any> | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      queryKeyComputed = options.queryKey;
      return {
        data: ref(mockDashboardStats),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    useDashboardStats(familyIdRef, { useQuery: useQuery as any, getDashboardService: () => mockDashboardService });

    expect(queryKeyComputed?.value).toEqual(queryKeys.dashboard.stats('family1'));
  });
});
