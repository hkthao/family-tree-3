// apps/mobile/family_tree_rn/stores/usePublicFamilyStore.ts

import { create } from 'zustand';
import {
  getPublicFamilyById,
  searchPublicFamilies,
} from '../src/api/publicApiClient';
import type { FamilyDetailDto, PaginatedList, FamilyListDto, SearchPublicFamiliesQuery } from '../src/types/public.d';

const PAGE_SIZE = 10; // Define PAGE_SIZE here

interface PublicFamilyState {
  family: FamilyDetailDto | null;
  families: FamilyListDto[]; // Changed to array for accumulation
  totalItems: number; // Added for pagination
  page: number; // Added
  totalPages: number; // Added
  loading: boolean;
  error: string | null;
  hasMore: boolean; // Add hasMore property
}

interface PublicFamilyActions {
  getFamilyById: (id: string) => Promise<void>;
  fetchFamilies: (query: { page: number; search?: string }, isRefreshing?: boolean) => Promise<void>;
  clearFamily: () => void;
  reset: () => void; // Renamed clearFamilies to reset
  setError: (error: string | null) => void;
}

type PublicFamilyStore = PublicFamilyState & PublicFamilyActions;

export const usePublicFamilyStore = create<PublicFamilyStore>((set, get) => ({ // Added get
  family: null,
  families: [], // Initialize as empty array
  totalItems: 0, // Initialize totalItems
  page: 1, // Initialize page
  totalPages: 0, // Initialize totalPages
  loading: false,
  error: null,
  hasMore: true, // Initialize hasMore to true as per requirement

  getFamilyById: async (id: string) => {
    set({ loading: true, error: null });
    try {
      const family = await getPublicFamilyById(id);
      set({ family, loading: false });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch family', loading: false });
    }
  },

  fetchFamilies: async (query: { page: number; search?: string }, isRefreshing: boolean = false) => {
    set({ loading: true, error: null });
    try {
      const paginatedList = await searchPublicFamilies({
        page: query.page,
        itemsPerPage: PAGE_SIZE, // Use PAGE_SIZE from FamilySearchScreen
        searchTerm: query.search,
      });
      set((state) => ({
        families: isRefreshing ? paginatedList.items : [...state.families, ...paginatedList.items],
        totalItems: paginatedList.totalItems,
        page: paginatedList.page,
        totalPages: paginatedList.totalPages,
        hasMore: paginatedList.totalPages > 0 && paginatedList.page < paginatedList.totalPages,
      }));
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch family' });
    } finally {
      set({ loading: false }); // Ensure loading is always set to false
    }
  },

  clearFamily: () => set({ family: null }),
  reset: () => set({ families: [], totalItems: 0, page: 1, totalPages: 0, hasMore: true }), // Renamed and set hasMore to true
  setError: (error: string | null) => set({ error }),
}));