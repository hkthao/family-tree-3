import { create } from 'zustand';
import { eventService } from '@/services'; // Import the new eventService
import type { EventDto, PaginatedList, SearchPublicEventsQuery, GetPublicUpcomingEventsQuery } from '@/types';

interface EventState {
  event: EventDto | null;
  events: EventDto[];
  paginatedEvents: PaginatedList<EventDto> | null;
  upcomingEvents: EventDto[];
  loading: boolean;
  error: string | null;
  page: number; // Renamed from currentPage
  hasMore: boolean;
}

interface EventActions {
  getEventById: (id: string) => Promise<void>;
  fetchEvents: (familyId: string, query: SearchPublicEventsQuery, isLoadMore: boolean) => Promise<PaginatedList<EventDto> | null>;
  fetchUpcomingEvents: (query: GetPublicUpcomingEventsQuery) => Promise<EventDto[] | null>; // Changed return type
  reset: () => void;
  setError: (error: string | null) => void;
}

type EventStore = EventState & EventActions;

const PAGE_SIZE = 10; // Define page size for pagination

export const usePublicEventStore = create<EventStore>((set, get) => ({
  event: null,
  events: [],
  paginatedEvents: null,
  upcomingEvents: [],
  loading: false,
  error: null,
  page: 1, // Initialize page
  hasMore: true, // Initialize hasMore
  
    getEventById: async (id: string) => {
      set({ loading: true, error: null });
      try {
        const result = await eventService.getEventById(id);
        if (result.isSuccess && result.value) {
          set({ event: result.value });
        } else {
          set({ error: result.error?.message || 'Failed to fetch event' });
        }
      } catch (err: any) {
        set({ error: err.message || 'Failed to fetch event' });
      } finally {
        set({ loading: false });
      }
    },
  
    fetchEvents: async (familyId: string, query: SearchPublicEventsQuery, isLoadMore: boolean): Promise<PaginatedList<EventDto> | null> => {
      set({ loading: true, error: null });
      try {
        const pageNumber = isLoadMore ? get().page + 1 : 1; // Use get().page
        const result = await eventService.searchEvents({
          ...query,
          familyId: familyId,
          page: pageNumber,
          itemsPerPage: PAGE_SIZE,
        });

        if (result.isSuccess && result.value) {
          const response = result.value;
          const newEvents = response.items;
          set((state) => ({
            events: isLoadMore ? [...state.events, ...newEvents] : newEvents,
            paginatedEvents: response,
            page: pageNumber, // Update page here
            hasMore: response.page < response.totalPages,
          }));
          return response;
        } else {
          set({ error: result.error?.message || 'Failed to search events' });
          return null;
        }
      } catch (err: any) {
        set({ error: err.message || 'Failed to search events' });
        return null;
      } finally {
        set({ loading: false });
      }
    },
  
    fetchUpcomingEvents: async (query: GetPublicUpcomingEventsQuery): Promise<EventDto[] | null> => { // Changed return type
      set({ loading: true, error: null });
      try {
        const result = await eventService.getUpcomingEvents(query);
        if (result.isSuccess && result.value) {
          set({ upcomingEvents: result.value });
          return result.value;
        } else {
          set({ error: result.error?.message || 'Failed to fetch upcoming events' });
          return null;
        }
      } catch (err: any) {
        set({ error: err.message || 'Failed to fetch upcoming events' });
        return null;
      }
    },
  
    reset: () => set({ event: null, events: [], paginatedEvents: null, upcomingEvents: [], error: null, page: 1, hasMore: true }),
  setError: (error: string | null) => set({ error }),
}));
