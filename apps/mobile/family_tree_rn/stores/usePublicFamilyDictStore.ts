import { create } from 'zustand';
import { familyDictService } from '@/services'; // Import the new familyDictService
import type { FamilyDictDto, PaginatedList, FamilyDictFilter } from '@/types';

const PAGE_SIZE = 10;

interface PublicFamilyDictState {
  familyDict: FamilyDictDto | null; // Changed FamilyDictDto to FamilyDict
  familyDicts: FamilyDictDto[]; // Changed FamilyDictDto to FamilyDict
  totalItems: number;
  page: number;
  totalPages: number;
  loading: boolean;
  error: string | null;
  hasMore: boolean;
}

interface PublicFamilyDictActions {
  getFamilyDictById: (id: string) => Promise<void>;
  fetchFamilyDicts: (filter: FamilyDictFilter, page: number, itemsPerPage: number, isRefreshing?: boolean) => Promise<PaginatedList<FamilyDictDto> | null>;
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
      const result = await familyDictService.getFamilyDictById(id);
      if (result.isSuccess && result.value) {
        set({ familyDict: result.value, loading: false });
      } else {
        set({ error: result.error?.message || 'Failed to fetch family dictionary entry', loading: false });
      }
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch family dictionary entry', loading: false });
    }
  },

  fetchFamilyDicts: async (filter: FamilyDictFilter, page: number, itemsPerPage: number, isRefreshing: boolean = false): Promise<PaginatedList<FamilyDictDto> | null> => {
    set({ loading: true, error: null });
    try {
      const result = await familyDictService.getFamilyDicts(filter, page, itemsPerPage);
      if (result.isSuccess && result.value) {
        const paginatedList: PaginatedList<FamilyDictDto> = result.value;

        set((state) => ({
          familyDicts: isRefreshing ? paginatedList.items : [...state.familyDicts, ...paginatedList.items],
          totalItems: paginatedList.totalItems,
          page: paginatedList.page,
          totalPages: paginatedList.totalPages,
          hasMore: paginatedList.totalPages > 0 && paginatedList.page < paginatedList.totalPages,
        }));
        return paginatedList;
      } else {
        set({ error: result.error?.message || 'Failed to fetch family dictionary entries' });
        return null;
      }
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch family dictionary entries' });
      return null;
    } finally {
      set({ loading: false });
    }
  },

  clearFamilyDict: () => set({ familyDict: null }),
  reset: () => set({ familyDicts: [], totalItems: 0, page: 1, totalPages: 0, hasMore: true }),
  setError: (error: string | null) => set({ error }),
}));