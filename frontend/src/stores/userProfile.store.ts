import { defineStore } from 'pinia';
import type { UserProfile } from '@/types';

interface UserProfileState {
    items: UserProfile[];
    loading: boolean;
    error: string | null;
}

export const useUserProfileStore = defineStore('userProfile', {
    state: (): UserProfileState => ({
        items: [],
        loading: false,
        error: null,
    }),
    actions: {
        async fetchUserProfiles() {
            this.loading = true;
            this.error = null;
            try {
                const response = await this.services.userProfile.getAllUserProfiles();
                if (response.ok) {
                    this.items = response.value!;
                } else {
                    this.error = response.error?.message || 'Failed to fetch user profiles.';
                }
            } catch (error) {
                this.error = (error as Error).message;
            } finally {
                this.loading = false;
            }
        },
    },
    getters: {
        allUserProfiles: (state) => state.items,
    },
});
