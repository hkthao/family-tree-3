import { defineStore } from 'pinia';
import { useAuthService } from '@/services/auth/authService';
import type { User } from '@/types';
import { useUserProfileStore } from './userProfile.store';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as User | null,
    token: null as string | null,
    loading: false,
    error: null as string | null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.user,
    isAdmin: (state) => state.user?.roles?.includes('Admin'),
    isFamilyManager: (state) => state.user?.roles?.includes('FamilyManager'),
    getAccessToken: (state) => state.token,
  },
  actions: {
    async initAuth() {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        const authUser = await authService.getUser(); // This will be the Auth0 user data
        this.token = await authService.getAccessToken();

        if (authUser) {
          const userProfileStore = useUserProfileStore();
          await userProfileStore.fetchUserProfileByExternalId(authUser.externalId);

          if (userProfileStore.userProfile) {
            this.user = {
              ...authUser,
              id: userProfileStore.userProfile.id, // Use internal UserProfile ID
              name: userProfileStore.userProfile.name, // Update name from profile
              email: userProfileStore.userProfile.email, // Update email from profile
              avatar: userProfileStore.userProfile.avatar || undefined, // Update avatar from profile
            };
          } else {
            // Handle case where user profile is not found in our DB
            console.error('User profile not found in DB for external ID:', authUser.externalId);
            this.error = 'User profile not found.';
            this.user = null;
          }
        } else {
          this.user = null;
        }
      } catch (err: any) {
        this.error = err.message || 'Failed to initialize authentication.';
        console.error(err);
      } finally {
        this.loading = false;
      }
    },

    async login() {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        // Auth0 login initiates a redirect, so we don't get a user back immediately.
        // The user will be set after the Auth0 callback.
        await authService.login({ appState: { target: '/' } }); // Redirect to home after login
      } catch (err: any) {
        this.error = err.message || 'Login failed.';
        console.error(err);
      } finally {
        this.loading = false;
      }
    },

    async logout() {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        await authService.logout();
        this.user = null;
        this.token = null;
      } catch (err: any) {
        this.error = err.message || 'Logout failed.';
        console.error(err);
      } finally {
        this.loading = false;
      }
    },

    async register(data: any) {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        this.user = await authService.register(data);
        this.token = await authService.getAccessToken();
        if (!this.user) {
          this.error = 'Registration failed.';
        }
      } catch (err: any) {
        this.error = err.message || 'Registration failed.';
        console.error(err);
      } finally {
        this.loading = false;
      }
    },
  },
});
