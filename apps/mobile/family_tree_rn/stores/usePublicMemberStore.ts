// apps/mobile/family_tree_rn/stores/usePublicMemberStore.ts

import { create } from 'zustand';
import { searchPublicMembers, getPublicMemberById } from '@/api/publicApiClient';
import type { MemberListDto, SearchPublicMembersQuery, MemberDetailDto } from '@/types';


interface PublicMemberState {
  member: MemberDetailDto | null; // Added for member details
  members: MemberListDto[];
  page: number;
  totalPages: number;
  totalItems: number;
  loading: boolean;
  error: string | null;
  hasMore: boolean;
}

interface PublicMemberActions {
  getMemberById: (id: string, familyId: string) => Promise<MemberDetailDto | null>; // Added for member details
  fetchMembers: (query: SearchPublicMembersQuery, isRefreshing?: boolean) => Promise<void>;
  reset: () => void;
  setError: (error: string | null) => void;
}

type PublicMemberStore = PublicMemberState & PublicMemberActions;

export const usePublicMemberStore = create<PublicMemberStore>((set, get) => ({
  member: null, // Initialize member
  members: [],
  page: 1,
  totalPages: 0,
  totalItems: 0,
  loading: false,
  error: null,
  hasMore: true,

  getMemberById: async (id: string, familyId: string) => {
    set({ loading: true, error: null });
    try {
      const member = await getPublicMemberById(id, familyId);
      set({ member });
      return member;
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch member' });
      return null;
    } finally {
      set({ loading: false });
    }
  },

  fetchMembers: async (query: SearchPublicMembersQuery, isRefreshing: boolean = false) => {
    set({ loading: true, error: null });
    try {
      const paginatedList = await searchPublicMembers(query);
      set((state) => ({
        members: isRefreshing ? paginatedList.items : [...state.members, ...paginatedList.items],
        totalItems: paginatedList.totalItems,
        page: paginatedList.page,
        totalPages: paginatedList.totalPages,
        hasMore: paginatedList.totalPages > 0 && paginatedList.page < paginatedList.totalPages,
      }));
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch members' });
    } finally {
      set({ loading: false });
    }
  },

  reset: () => set({ members: [], page: 1, totalPages: 0, totalItems: 0, hasMore: true, error: null }),
  setError: (error: string | null) => set({ error }),
}));