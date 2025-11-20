import axios from 'axios';
import type { EventDto, PaginatedList, GetEventsQuery, SearchEventsQuery, GetUpcomingEventsQuery } from '../types/public.d';
import Constants from 'expo-constants';

const API_BASE_URL = Constants.expoConfig?.extra?.apiBaseUrl;
const API_URL = `${API_BASE_URL}/event`;

export const getEvents = async (query: GetEventsQuery): Promise<EventDto[]> => {
  const response = await axios.get<EventDto[]>(API_URL, { params: query });
  return response.data;
};

export const getEventById = async (id: string): Promise<EventDto> => {
  const response = await axios.get<EventDto>(`${API_URL}/${id}`);
  return response.data;
};

export const searchEvents = async (query: SearchEventsQuery): Promise<PaginatedList<EventDto>> => {
  const response = await axios.get<PaginatedList<EventDto>>(`${API_URL}/search`, { params: query });
  return response.data;
};

export const getUpcomingEvents = async (query: GetUpcomingEventsQuery): Promise<EventDto[]> => {
  const response = await axios.get<EventDto[]>(`${API_URL}/upcoming`, { params: query });
  return response.data;
};
