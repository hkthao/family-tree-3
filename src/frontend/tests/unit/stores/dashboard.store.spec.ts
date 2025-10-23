import { setActivePinia, createPinia } from 'pinia';
import { useDashboardStore } from '@/stores/dashboard.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { EventType, type DashboardStats, type Event } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IDashboardService and IEventService
const mockFetchStats = vi.fn();
const mockGetUpcomingEvents = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    dashboard: {
      fetchStats: mockFetchStats,
    },
    event: {
      getUpcomingEvents: mockGetUpcomingEvents,
    },
    // Add other services as empty objects if they are not directly used by dashboard.store
    ai: {},
    auth: {},
    chat: {},
    chunk: {},
    face: {},
    faceMember: {},
    family: {},
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('dashboard.store', () => {
  let store: ReturnType<typeof useDashboardStore>;

  const mockDashboardStats: DashboardStats = {
    totalFamilies: 5,
    totalMembers: 100,
    totalRelationships: 200,
    totalGenerations: 5,
  };

  const mockEvent: Event = {
    id: 'event-1',
    familyId: 'family-1',
    name: 'Test Event',
    description: 'An event for testing',
    type: EventType.Other,
    startDate: new Date(),
  };

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useDashboardStore();
    store.$reset();
    store.upcomingEvents = []; // Ensure upcomingEvents is empty initially
    // Manually inject the mocked services
    // @ts-expect-error: Mocking services for testing
    store.services = createServices('mock');

    // Reset mocks before each test
    mockFetchStats.mockReset();
    mockGetUpcomingEvents.mockReset();

    // Set default mock resolved values
    mockFetchStats.mockResolvedValue(ok(mockDashboardStats));
    mockGetUpcomingEvents.mockResolvedValue(ok([mockEvent]));
  });

  it('should have correct initial state', () => {
    expect(store.stats).toBeNull();
    expect(store.upcomingEvents).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('fetchDashboardStats', () => {
    it('should fetch dashboard stats successfully', async () => {
      const result = await store.fetchDashboardStats();

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.stats).toEqual(mockDashboardStats);
      expect(mockFetchStats).toHaveBeenCalledTimes(1);
    });

    it('should handle fetch dashboard stats failure', async () => {
      const errorMessage = 'Failed to fetch stats.';
      mockFetchStats.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.fetchDashboardStats();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.stats).toBeNull();
      expect(mockFetchStats).toHaveBeenCalledTimes(1);
    });
  });

  describe('fetchUpcomingEvents', () => {
    it('should fetch upcoming events successfully', async () => {
      const result = await store.fetchUpcomingEvents();

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.upcomingEvents).toEqual([mockEvent]);
      expect(mockGetUpcomingEvents).toHaveBeenCalledTimes(1);
    });

    it('should handle fetch upcoming events failure', async () => {
      const errorMessage = 'Failed to fetch upcoming events.';
      mockGetUpcomingEvents.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.fetchUpcomingEvents();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.upcomingEvents).toEqual([]);
      expect(mockGetUpcomingEvents).toHaveBeenCalledTimes(1);
    });
  });

  describe('fetchAllDashboardData', () => {
    it('should fetch all dashboard data successfully', async () => {
      const result = await store.fetchAllDashboardData();

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.stats).toEqual(mockDashboardStats);
      expect(store.upcomingEvents).toEqual([mockEvent]);
      expect(mockFetchStats).toHaveBeenCalledTimes(1);
      expect(mockGetUpcomingEvents).toHaveBeenCalledTimes(1);
    });

    it('should handle fetch all dashboard data failure if stats fetch fails', async () => {
      const errorMessage = 'Failed to fetch stats.';
      mockFetchStats.mockResolvedValue(err({ message: errorMessage } as ApiError));
      mockGetUpcomingEvents.mockResolvedValue(ok([])); // Ensure upcomingEvents is empty

      const result = await store.fetchAllDashboardData();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.stats).toBeNull();
      expect(store.upcomingEvents).toEqual([]);
      expect(mockFetchStats).toHaveBeenCalledTimes(1);
      expect(mockGetUpcomingEvents).toHaveBeenCalledTimes(1); // Still called in Promise.all
    });

    it('should handle fetch all dashboard data failure if events fetch fails', async () => {
      const errorMessage = 'Failed to fetch upcoming events.';
      mockGetUpcomingEvents.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.fetchAllDashboardData();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.stats).toEqual(mockDashboardStats);
      expect(store.upcomingEvents).toEqual([]);
      expect(mockFetchStats).toHaveBeenCalledTimes(1);
      expect(mockGetUpcomingEvents).toHaveBeenCalledTimes(1);
    });

    it('should handle unexpected error during fetch all dashboard data', async () => {
      const errorMessage = 'Network error.';
      mockFetchStats.mockRejectedValue(new Error(errorMessage));
      mockGetUpcomingEvents.mockResolvedValue(ok([])); // Ensure upcomingEvents is empty

      const result = await store.fetchAllDashboardData();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.stats).toBeNull();
      expect(store.upcomingEvents).toEqual([]);
      expect(mockFetchStats).toHaveBeenCalledTimes(1);
      expect(mockGetUpcomingEvents).toHaveBeenCalledTimes(1);
    });
  });
});