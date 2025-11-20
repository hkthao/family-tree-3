// apps/mobile/family_tree_rn/stores/usePublicRelationshipStore.ts

import { create } from 'zustand';
import { getPublicRelationshipsByFamilyId } from '@/api/publicApiClient';
import type { RelationshipListDto } from '@/types/public.d';

interface PublicRelationshipState {
  relationships: RelationshipListDto[];
  loading: boolean;
  error: string | null;
}

interface PublicRelationshipActions {
  getRelationshipsByFamilyId: (familyId: string) => Promise<void>;
  clearRelationships: () => void;
  setError: (error: string | null) => void;
}

type PublicRelationshipStore = PublicRelationshipState & PublicRelationshipActions;

export const usePublicRelationshipStore = create<PublicRelationshipStore>((set) => ({
  relationships: [],
  loading: false,
  error: null,

  getRelationshipsByFamilyId: async (familyId: string) => {
    set({ loading: true, error: null });
    try {
      const relationships = await getPublicRelationshipsByFamilyId(familyId);
      set({ relationships, loading: false });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch relationships', loading: false });
    }
  },

  clearRelationships: () => set({ relationships: [] }),
  setError: (error: string | null) => set({ error }),
}));
