import type { FamilyFilter, FamilyMediaFilter, ListOptions, FamilyDictFilter, FilterOptions, EventFilter } from '@/types';

export const queryKeys = {
  dashboard: {
    all: ['dashboard'] as const,
    stats: (familyId?: string) => [...queryKeys.dashboard.all, 'stats', familyId] as const,
  },
  events: {
    all: ['events'] as const,
    list: (filters?: EventFilter) => [...queryKeys.events.all, 'list', filters] as const,
    detail: (eventId: string) => [...queryKeys.events.all, 'detail', eventId] as const,
  },
  userActivity: {
    all: ['userActivity'] as const,
    recent: (familyId?: string, page?: number, itemsPerPage?: number, targetType?: string, targetId?: string) => [...queryKeys.userActivity.all, 'recent', familyId, page, itemsPerPage, targetType, targetId] as const,
  },
  auth: {
    accessToken: ['auth', 'accessToken'] as const,
  },
  userSettings: {
    preferences: ['userSettings', 'preferences'] as const,
  },
  families: {
    all: ['families'] as const,
    list: (filters?: FamilyFilter) => [...queryKeys.families.all, 'list', filters] as const,
    detail: (familyId: string) => [...queryKeys.families.all, 'detail', familyId] as const,
    membersByFamilyId: (familyId: string) => [...queryKeys.families.all, familyId, 'members'] as const,
    relationshipsByFamilyId: (familyId: string) => [...queryKeys.families.all, familyId, 'relationships'] as const,
    memberRelationshipsByMemberId: (memberId: string) => [...queryKeys.families.all, memberId, 'relationships'] as const,
  },
  members: {
    all: ['members'] as const,
    detail: (memberId: string) => [...queryKeys.members.all, 'detail', memberId] as const,
  },
  privacyConfiguration: {
    detail: (familyId: string) => ['privacyConfiguration', familyId] as const,
  },
  memberFaces: {
    all: ['memberFaces'] as const,
    list: (options?: ListOptions, filters?: FilterOptions) => [...queryKeys.memberFaces.all, 'list', options, filters] as const,
    detail: (id: string) => [...queryKeys.memberFaces.all, 'detail', id] as const,
  },
  familyMedia: {
    all: ['familyMedia'] as const,
    list: (filters?: FamilyMediaFilter, page?: number, itemsPerPage?: number, sortBy?: ListOptions['sortBy']) => [...queryKeys.familyMedia.all, 'list', filters, page, itemsPerPage, sortBy] as const,
    detail: (mediaId: string) => [...queryKeys.familyMedia.all, 'detail', mediaId] as const,
  },
  familyDicts: {
    all: ['familyDicts'] as const,
    list: (filters?: FamilyDictFilter) => [...queryKeys.familyDicts.all, 'list', filters] as const,
    detail: (familyDictId: string) => [...queryKeys.familyDicts.all, 'detail', familyDictId] as const,
  },
  users: {
    all: ['users'] as const,
    search: (searchQuery: string) => [...queryKeys.users.all, 'search', searchQuery] as const,
    byIds: (ids: string[]) => [...queryKeys.users.all, 'byIds', ids] as const,
  },
};
