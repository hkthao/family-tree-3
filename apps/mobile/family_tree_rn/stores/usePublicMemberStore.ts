// apps/mobile/family_tree_rn/stores/usePublicMemberStore.ts

import { create } from 'zustand';
import {
  getPublicMembersByFamilyId,
  getPublicMemberById,
} from '../src/api/publicApiClient';
import type { MemberListDto, MemberDetailDto } from '../src/types/public.d';

interface PublicMemberState {
  members: MemberListDto[];
  member: MemberDetailDto | null;
  loading: boolean;
  error: string | null;
}

interface PublicMemberActions {
  getMembersByFamilyId: (familyId: string) => Promise<void>;
  getMemberById: (id: string, familyId: string) => Promise<void>;
  clearMembers: () => void;
  clearMember: () => void;
  setError: (error: string | null) => void;
}

type PublicMemberStore = PublicMemberState & PublicMemberActions;

export const usePublicMemberStore = create<PublicMemberStore>((set) => ({
  members: [],
  member: null,
  loading: false,
  error: null,

  getMembersByFamilyId: async (familyId: string) => {
    set({ loading: true, error: null });
    try {
      const members = await getPublicMembersByFamilyId(familyId);
      set({ members, loading: false });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch members', loading: false });
    }
  },

  getMemberById: async (id: string, familyId: string) => {
    set({ loading: true, error: null });
    try {
      const member = await getPublicMemberById(id, familyId);
      set({ member, loading: false });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch member details', loading: false });
    }
  },

  clearMembers: () => set({ members: [] }),
  clearMember: () => set({ member: null }),
  setError: (error: string | null) => set({ error }),
}));
