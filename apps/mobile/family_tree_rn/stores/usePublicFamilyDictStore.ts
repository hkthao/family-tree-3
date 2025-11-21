import { create } from 'zustand';
import {
  getPublicFamilyDictById,
  getPublicFamilyDicts,
} from '@/api/publicFamilyDictApiClient';
import type { FamilyDictDto, PaginatedFamilyDictDto, FamilyDictFilter } from '@/types/public.d';

const PAGE_SIZE = 10;

interface PublicFamilyDictState {
  familyDict: FamilyDictDto | null;
  familyDicts: FamilyDictDto[];
  totalItems: number;
  page: number;
  totalPages: number;
  loading: boolean;
  error: string | null;
  hasMore: boolean;
}

interface PublicFamilyDictActions {
  getFamilyDictById: (id: string) => Promise<void>;
  fetchFamilyDicts: (filter: FamilyDictFilter, page: number, itemsPerPage: number, isRefreshing?: boolean) => Promise<void>;
  clearFamilyDict: () => void;
  reset: () => void;
  setError: (error: string | null) => void;
}

type PublicFamilyDictStore = PublicFamilyDictState & PublicFamilyDictActions;

export const usePublicFamilyDictStore = create<PublicFamilyDictStore>((set, get) => ({
  familyDict: null,
  familyDicts: [],
  totalItems: 0,
  page: 1,
  totalPages: 0,
  loading: false,
  error: null,
  hasMore: true,

  getFamilyDictById: async (id: string) => {
    set({ loading: true, error: null });
    try {
      const familyDict = await getPublicFamilyDictById(id);
      set({ familyDict, loading: false });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch family dictionary entry', loading: false });
    }
  },

  fetchFamilyDicts: async (filter: FamilyDictFilter, page: number, itemsPerPage: number, isRefreshing: boolean = false) => {
    set({ loading: true, error: null });
    try {
      const paginatedList: PaginatedFamilyDictDto = await getPublicFamilyDicts(filter, page, itemsPerPage);

      set((state) => ({
        familyDicts: isRefreshing ? paginatedList.items : [...state.familyDicts, ...paginatedList.items],
        totalItems: paginatedList.totalItems,
        page: paginatedList.page,
        totalPages: paginatedList.totalPages,
        hasMore: paginatedList.totalPages > 0 && paginatedList.page < paginatedList.totalPages,
      }));
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch family dictionary entries' });
    } finally {
      set({ loading: false });
    }
  },

  clearFamilyDict: () => set({ familyDict: null }),
  reset: () => set({ familyDicts: [], totalItems: 0, page: 1, totalPages: 0, hasMore: true }),
  setError: (error: string | null) => set({ error }),
}));