// apps/mobile/family_tree_rn/stores/useUserProfileStore.ts

import { create } from 'zustand';
import { UserProfileDto } from '@/types'; // Import from central types index
import { userProfileService } from '@/services'; // Import the userProfileService

interface UserProfileState {
  userProfile: UserProfileDto | null;
  loading: boolean;
  error: string | null;
}

interface UserProfileActions {
  fetchUserProfile: () => Promise<void>;
  clearUserProfile: () => void;
  setError: (error: string | null) => void;
}

type UserProfileStore = UserProfileState & UserProfileActions;

export const useUserProfileStore = create<UserProfileStore>((set, get) => ({
  userProfile: null,
  loading: false,
  error: null,

  fetchUserProfile: async () => {
    set({ loading: true, error: null });
    const result = await userProfileService.getCurrentUserProfile();
    if (result.isSuccess && result.value) {
      set({ userProfile: result.value, loading: false });
    } else {
      set({ error: result.error?.message || 'Failed to fetch user profile.', loading: false });
    }
  },

  clearUserProfile: () => set({ userProfile: null, error: null }),
  setError: (error: string | null) => set({ error }),
}));
