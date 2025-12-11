import type { FamilyFilter } from '@/types';

export const queryKeys = {
  dashboard: {
    all: ['dashboard'] as const,
    stats: (familyId?: string) => [...queryKeys.dashboard.all, 'stats', familyId] as const,
  },
  events: {
    all: ['events'] as const,
    upcoming: (familyId?: string) => [...queryKeys.events.all, 'upcoming', familyId] as const,
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
  },
};

