// apps/mobile/family_tree_rn/src/api/publicApiClient.ts

import axios from 'axios';
import type {
  FamilyDetailDto,
  PaginatedList,
  FamilyListDto,
  MemberListDto,
  MemberDetailDto,
  RelationshipListDto,
  SearchPublicFamiliesQuery,
  SearchPublicMembersQuery,
  EventDto, // Added
  SearchPublicEventsQuery, // Added
  GetPublicUpcomingEventsQuery, // Added
} from '../types/public.d';

// TODO: Configure this based on your environment (e.g., .env file)
const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL; // Example: Replace with your backend URL

const publicApiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const getPublicFamilyById = async (id: string): Promise<FamilyDetailDto> => {
  const response = await publicApiClient.get<FamilyDetailDto>(`/family/${id}`);
  return response.data;
};

export const searchPublicFamilies = async (
  query: SearchPublicFamiliesQuery
): Promise<PaginatedList<FamilyListDto>> => {
  const response = await publicApiClient.get<PaginatedList<FamilyListDto>>('/families/search', {
    params: query,
  });
  return response.data;
};

export const searchPublicMembers = async (
  query: SearchPublicMembersQuery
): Promise<PaginatedList<MemberListDto>> => {
  const response = await publicApiClient.get<PaginatedList<MemberListDto>>('/members/search', {
    params: query,
  });
  return response.data;
};

export const getPublicMembersByFamilyId = async (familyId: string): Promise<MemberListDto[]> => {
  const response = await publicApiClient.get<MemberListDto[]>(`/family/${familyId}/members`);
  return response.data;
};

export const getPublicMemberById = async (
  id: string,
  familyId: string
): Promise<MemberDetailDto> => {
  const response = await publicApiClient.get<MemberDetailDto>(`/family/${familyId}/member/${id}`, {
    params: { familyId },
  });
  return response.data;
};

export const getPublicEventById = async (id: string): Promise<EventDto> => {
  const response = await publicApiClient.get<EventDto>(`/event/${id}`);
  return response.data;
};

export const searchPublicEvents = async (query: SearchPublicEventsQuery): Promise<PaginatedList<EventDto>> => {
  const response = await publicApiClient.get<PaginatedList<EventDto>>('/events/search', {
    params: query,
  });
  return response.data;
};

export const getPublicUpcomingEvents = async (query: GetPublicUpcomingEventsQuery): Promise<EventDto[]> => {
  const response = await publicApiClient.get<EventDto[]>(`/events/upcoming`, { params: query });
  return response.data;
};

export const getPublicRelationshipsByFamilyId = async (
  familyId: string
): Promise<RelationshipListDto[]> => {
  const response = await publicApiClient.get<RelationshipListDto[]>(
    `/family/${familyId}/relationships`
  );
  return response.data;
};
