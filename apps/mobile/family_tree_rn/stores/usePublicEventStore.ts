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
}

interface EventActions {
  getEventById: (id: string) => Promise<void>;
  fetchEvents: (query: SearchPublicEventsQuery) => Promise<PaginatedList<EventDto> | null>; // Renamed and type changed
  fetchUpcomingEvents: (query: GetPublicUpcomingEventsQuery) => Promise<void>; // Renamed
  reset: () => void;
  setError: (error: string | null) => void;
}

type EventStore = EventState & EventActions;

export const usePublicEventStore = create<EventStore>((set) => ({
  event: null,
  events: [],
  paginatedEvents: null,
  upcomingEvents: [],
  loading: false,
  error: null,

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
  
    fetchEvents: async (query: SearchPublicEventsQuery): Promise<PaginatedList<EventDto> | null> => { // Renamed from searchEvents
      set({ loading: true, error: null });
      try {
        const paginatedEvents: PaginatedList<EventDto> = await searchPublicEvents(query);
        set({ paginatedEvents });
        return paginatedEvents; // Return the fetched events
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
  
    reset: () => set({ event: null, paginatedEvents: null, upcomingEvents: [], error: null }), // Removed 'events'
  setError: (error: string | null) => set({ error }),
}));
