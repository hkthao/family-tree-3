// apps/mobile/family_tree_rn/stores/usePublicRelationshipStore.ts

import { create } from 'zustand';
import { relationshipService } from '@/services'; // Import the new relationshipService
import type { RelationshipListDto } from '@/types';

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
      const result = await relationshipService.getRelationshipsByFamilyId(familyId);
      if (result.isSuccess && result.value) {
        set({ relationships: result.value, loading: false });
      } else {
        set({ error: result.error?.message || 'Failed to fetch relationships', loading: false });
      }
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch relationships', loading: false });
    }
  },

  clearRelationships: () => set({ relationships: [] }),
  setError: (error: string | null) => set({ error }),
}));
