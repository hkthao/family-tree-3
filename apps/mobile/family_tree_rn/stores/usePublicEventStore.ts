import { create } from 'zustand';
import { getPublicEventById, searchPublicEvents, getPublicUpcomingEvents } from '@/api/publicApiClient';
import type { EventDto, PaginatedList, SearchPublicEventsQuery, GetPublicUpcomingEventsQuery } from '@/types';

interface EventState {
  event: EventDto | null;
  events: EventDto[];
  paginatedEvents: PaginatedList<EventDto> | null;
  upcomingEvents: EventDto[];
  loading: boolean;
  error: string | null;
  currentPage: number;
  hasMore: boolean;
}

interface EventActions {
  getEventById: (id: string) => Promise<void>;
  fetchEvents: (familyId: string, query: SearchPublicEventsQuery, isLoadMore: boolean) => Promise<PaginatedList<EventDto> | null>; // Renamed and type changed
  fetchUpcomingEvents: (query: GetPublicUpcomingEventsQuery) => Promise<void>; // Renamed
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
  currentPage: 1, // Initialize current page
  hasMore: true, // Initialize hasMore
  
    getEventById: async (id: string) => {
      set({ loading: true, error: null });
      try {
        const event = await getPublicEventById(id);
        set({ event });
      } catch (err: any) {
        set({ error: err.message || 'Failed to fetch event' });
      } finally {
        set({ loading: false });
      }
    },
  
    fetchEvents: async (familyId: string, query: SearchPublicEventsQuery, isLoadMore: boolean): Promise<PaginatedList<EventDto> | null> => {
      set({ loading: true, error: null });
      try {
        const pageNumber = isLoadMore ? get().currentPage + 1 : 1;
        const response = await searchPublicEvents({
          ...query,
          familyId: familyId,
          page: pageNumber,
          itemsPerPage: PAGE_SIZE,
        });

        if (response) {
          const newEvents = response.items;
          set((state) => ({
            events: isLoadMore ? [...state.events, ...newEvents] : newEvents,
            paginatedEvents: response,
            currentPage: pageNumber,
            hasMore: response.page < response.totalPages,
          }));
        }
        return response;
      } catch (err: any) {
        set({ error: err.message || 'Failed to search events' });
        return null;
      } finally {
        set({ loading: false });
      }
    },
  
    fetchUpcomingEvents: async (query: GetPublicUpcomingEventsQuery) => { // Renamed from fetchUpcomingEvents
      set({ loading: true, error: null });
      try {
        const upcomingEvents = await getPublicUpcomingEvents(query);
        set({ upcomingEvents });
      } catch (err: any) {
        set({ error: err.message || 'Failed to fetch upcoming events' });
      } finally {
        set({ loading: false });
      }
    },
  
    reset: () => set({ event: null, events: [], paginatedEvents: null, upcomingEvents: [], error: null, currentPage: 1, hasMore: true }), // Removed 'events'
  setError: (error: string | null) => set({ error }),
}));
