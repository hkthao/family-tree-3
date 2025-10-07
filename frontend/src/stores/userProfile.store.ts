import { defineStore } from 'pinia';
import type { UserProfile } from '@/types/user';

interface UserProfileState {
    userProfiles: UserProfile[];
    loading: boolean;
    error: string | null;
}

export const useUserProfileStore = defineStore('userProfile', {
    state: (): UserProfileState => ({
        userProfiles: [],
        loading: false,
        error: null,
    }),
    actions: {
        async fetchUserProfiles() {
            this.loading = true;
            this.error = null;
            try {
                // TODO: Implement API call to fetch user profiles
                // For now, we'll use mock data.
                this.userProfiles = [
                    { id: '1', auth0UserId: 'auth0|1', email: 'user1@example.com', name: 'User One' },
                    { id: '2', auth0UserId: 'auth0|2', email: 'user2@example.com', name: 'User Two' },
                ];
            } catch (error) {
                this.error = (error as Error).message;
            } finally {
                this.loading = false;
            }
        },
    },
    getters: {
        allUserProfiles: (state) => state.userProfiles,
    },
});
