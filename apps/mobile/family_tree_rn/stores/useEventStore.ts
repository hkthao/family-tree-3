import { create } from 'zustand';
import { getEventById, getEvents, searchEvents, getUpcomingEvents } from '../src/api/eventApiClient';
import type { EventDto, PaginatedList, GetEventsQuery, SearchEventsQuery, GetUpcomingEventsQuery } from '../src/types/public.d';

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
  fetchEvents: (query: GetEventsQuery) => Promise<void>;
  searchEvents: (query: SearchEventsQuery) => Promise<void>;
  fetchUpcomingEvents: (query: GetUpcomingEventsQuery) => Promise<void>;
  reset: () => void;
  setError: (error: string | null) => void;
}

type EventStore = EventState & EventActions;

export const useEventStore = create<EventStore>((set) => ({
  event: null,
  events: [],
  paginatedEvents: null,
  upcomingEvents: [],
  loading: false,
  error: null,

  getEventById: async (id: string) => {
    set({ loading: true, error: null });
    try {
      const event = await getEventById(id);
      set({ event });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch event' });
    } finally {
      set({ loading: false });
    }
  },

  fetchEvents: async (query: GetEventsQuery) => {
    set({ loading: true, error: null });
    try {
      const events = await getEvents(query);
      set({ events });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch events' });
    } finally {
      set({ loading: false });
    }
  },

  searchEvents: async (query: SearchEventsQuery) => {
    set({ loading: true, error: null });
    try {
      const paginatedEvents = await searchEvents(query);
      set({ paginatedEvents });
    } catch (err: any) {
      set({ error: err.message || 'Failed to search events' });
    } finally {
      set({ loading: false });
    }
  },

  fetchUpcomingEvents: async (query: GetUpcomingEventsQuery) => {
    set({ loading: true, error: null });
    try {
      const upcomingEvents = await getUpcomingEvents(query);
      set({ upcomingEvents });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch upcoming events' });
    } finally {
      set({ loading: false });
    }
  },

  reset: () => set({ event: null, events: [], paginatedEvents: null, upcomingEvents: [], error: null }),
  setError: (error: string | null) => set({ error }),
}));
