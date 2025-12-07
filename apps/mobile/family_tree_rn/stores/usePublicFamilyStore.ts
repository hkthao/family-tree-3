// apps/mobile/family_tree_rn/stores/usePublicFamilyStore.ts

import { create } from 'zustand';
import { familyService } from '@/services'; // Import the new familyService
import type { FamilyDetailDto, FamilyListDto, PaginatedList } from '@/types';

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
  fetchFamilies: (query: { page: number; search?: string }, isRefreshing?: boolean) => Promise<PaginatedList<FamilyListDto> | null>;
  clearFamily: () => void;
  reset: () => void;
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
      const result = await familyService.getFamilyById(id);
      if (result.isSuccess && result.value) {
        set({ family: result.value, loading: false });
      } else {
        set({ error: result.error?.message || 'Failed to fetch family', loading: false });
      }
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch family', loading: false });
    }
  },

  fetchFamilies: async (query: { page: number; search?: string }, isRefreshing: boolean = false): Promise<PaginatedList<FamilyListDto> | null> => {
    set({ loading: true, error: null });
    try {
      const result = await familyService.searchFamilies({ // Updated type
        page: query.page,
        itemsPerPage: PAGE_SIZE, // Use PAGE_SIZE from FamilySearchScreen
        searchTerm: query.search,
      });

      if (result.isSuccess && result.value) {
        const paginatedList: PaginatedList<FamilyListDto> = result.value;
        
        set((state) => ({
          families: isRefreshing ? paginatedList.items : [...(state.families || []), ...paginatedList.items],
          totalItems: paginatedList.totalItems,
          page: paginatedList.page,
          totalPages: paginatedList.totalPages,
          hasMore: paginatedList.totalPages > 0 && paginatedList.page < paginatedList.totalPages,
        }));
        return paginatedList;
      } else {
        console.error('Error fetching families:', result.error);
        set({ error: result.error?.message || 'Failed to fetch family' });
        return null;
      }
    } catch (err: any) {
      console.error('Error fetching families:', err);
      set({ error: err.message || 'Failed to fetch family' });
      return null;
    } finally {
      set({ loading: false }); // Ensure loading is always set to false
    }
  },

  clearFamily: () => set({ family: null }),
  reset: () => set({ families: [], totalItems: 0, page: 1, totalPages: 0, hasMore: true }), // Renamed and set hasMore to true
  setError: (error: string | null) => set({ error }),
}));
