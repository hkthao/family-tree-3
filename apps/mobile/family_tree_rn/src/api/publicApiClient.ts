// apps/mobile/family_tree_rn/src/api/publicApiClient.ts

import axios from 'axios';
import type {
  PaginatedList, // From public.d.ts
  FamilyDictDto, // From public.d.ts
  FamilyDictFilter, // From public.d.ts
  PublicDashboardDto, // From public-dashboard.d.ts
  FamilyDetailDto, // From public-family.d.ts
  FamilyListDto, // From public-family.d.ts
  SearchPublicFamiliesQuery, // From public-family.d.ts
  MemberListDto, // From public-member.d.ts
  MemberDetailDto, // From public-member.d.ts
  SearchPublicMembersQuery, // From public-member.d.ts
  RelationshipListDto, // From public-relationship.d.ts
  EventDto, // From public-event.d.ts
  SearchPublicEventsQuery, // From public-event.d.ts
  GetPublicUpcomingEventsQuery, // From public-event.d.ts
  FaceDetectionResponseDto, // From public-face.d.ts
  DetectFacesRequest, // From public-face.d.ts (moved from publicApiClient.ts)
} from '@/types'; // Import all types from admin's global types

// TODO: Configure this based on your environment (e.g., .env file)
const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL+'/api/public'; // Example: Replace with your backend URL
export const publicApiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to include the API Key
publicApiClient.interceptors.request.use(
  (config) => {
    const apiKey = process.env.EXPO_PUBLIC_API_KEY;
    if (apiKey) {
      config.headers['X-App-Key'] = apiKey; // Add the API Key header
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// --- Dashboard related types and functions ---
// Defined locally as it's a client-side derived metric interface
export interface DashboardMetrics {
  totalMembers: number;
  totalRelationships: number;
  totalGenerations: number;
  averageAge: number;
  livingMembers: number;
  deceasedMembers: number;
  genderDistribution: { name: string; population: number; color: string; legendFontColor: string; legendFontSize: number; }[];
  membersPerGeneration: { [key: number]: number };
  totalEvents: number;
}

// Helper function from dashboardService.ts
const getGenderColor = (gender: string): string => {
  switch (gender.toLowerCase()) {
    case 'male':
      return '#1E90FF'; // DodgerBlue
    case 'female':
      return '#FF69B4'; // HotPink
    default:
      return '#808080'; // Gray
  }
};

export const getPublicDashboardData = async (familyId: string): Promise<DashboardMetrics> => {
  const { data } = await publicApiClient.get<PublicDashboardDto>(`/dashboard`, {
    params: {
      familyId: familyId,
    },
  });

  return {
    totalMembers: data.totalPublicMembers,
    totalRelationships: data.totalPublicRelationships,
    totalGenerations: data.totalPublicGenerations,
    averageAge: data.publicAverageAge,
    livingMembers: data.publicLivingMembersCount,
    deceasedMembers: data.publicDeceasedMembersCount,
    genderDistribution: [
      {
        name: 'Male',
        population: Number((data.publicMaleRatio * 100).toFixed(2)), // Convert ratio to percentage and round for display
        color: getGenderColor('male'),
        legendFontColor: '#7F7F7F',
        legendFontSize: 15,
      },
      {
        name: 'Female',
        population: Number((data.publicFemaleRatio * 100).toFixed(2)), // Convert ratio to percentage and round for display
        color: getGenderColor('female'),
        legendFontColor: '#7F7F7F',
        legendFontSize: 15,
      },
    ],
    membersPerGeneration: data.publicMembersPerGeneration,
    totalEvents: data.totalPublicEvents,
  };
};
// --- End Dashboard related types and functions ---


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
): Promise<PaginatedList<MemberListDto>> => { // Changed PaginatedList to Paginated
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

export const searchPublicEvents = async (query: SearchPublicEventsQuery): Promise<PaginatedList<EventDto>> => { // Changed PaginatedList to Paginated
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

// Moved from publicApiClient.ts to public-face.d.ts
// export interface DetectFacesRequest {
//   imageBytes: string; // Base64 string of the image
//   contentType: string; // e.g., "image/jpeg"
//   returnCrop: boolean;
// }

export const detectFaces = async (
  request: DetectFacesRequest
): Promise<FaceDetectionResponseDto> => {
  const response = await publicApiClient.post<FaceDetectionResponseDto>(
    `/face/detect`,
    request
  );
  return response.data;
};

// --- Family Dict related functions ---
export const getPublicFamilyDicts = async (
  filter: FamilyDictFilter,
  page: number,
  itemsPerPage: number,
): Promise<PaginatedList<FamilyDictDto>> => { // Changed PaginatedFamilyDictDto to Paginated<FamilyDict>
  const response = await publicApiClient.get<PaginatedList<FamilyDictDto>>('/family-dict', { // Changed PaginatedFamilyDictDto to Paginated<FamilyDict>
    params: {
      page: page,
      itemsPerPage: itemsPerPage,
      searchQuery: filter.searchQuery,
      lineage: filter.lineage,
      region: filter.region,
      sortBy: filter.sortBy,
      sortOrder: filter.sortOrder,
    },
  });
  return response.data;
};

export const getPublicFamilyDictById = async (id: string): Promise<FamilyDictDto> => { // Changed FamilyDictDto to FamilyDict
  const response = await publicApiClient.get<FamilyDictDto>(`/family-dict/${id}`); // Changed FamilyDictDto to FamilyDict
  return response.data;
};
// --- End Family Dict related functions ---
