import { create } from 'zustand';

interface FamilyStore {
  currentFamilyId: string | null;
  setCurrentFamilyId: (id: string | null) => void;
}

export const useFamilyStore = create<FamilyStore>((set) => ({
  currentFamilyId: null,
  setCurrentFamilyId: (id) => set({ currentFamilyId: id }),
}));
