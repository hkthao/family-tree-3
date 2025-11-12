import { defineStore } from 'pinia';
import { useAuthService } from '@/services/auth/authService';
import type { User, Result } from '@/types';

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
    async initAuth(): Promise<Result<User | null, string>> {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        this.user = await authService.getUser(); // This will be the Auth0 user data
        this.token = await authService.getAccessToken();
        return { ok: true, value: this.user };
      } catch (err: any) {
        this.error = err.message || 'Failed to get user.';
        return { ok: false, error: this.error || 'Unknown error' };
      } finally {
        this.loading = false;
      }
    },

    async login(): Promise<Result<void, string>> {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        // Auth0 login initiates a redirect, so we don't get a user back immediately.
        // The user will be set after the Auth0 callback.
        await authService.login({ appState: { target: '/' } }); // Redirect to home after login
        return { ok: true, value: undefined };
      } catch (err: any) {
        this.error = err.message || 'Login failed.';
        console.error(err);
        return { ok: false, error: this.error || 'Unknown error' };
      } finally {
        this.loading = false;
      }
    },

    async logout(): Promise<Result<void, string>> {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        await authService.logout();
        this.user = null;
        this.token = null;
        return { ok: true, value: undefined };
      } catch (err: any) {
        this.error = err.message || 'Logout failed.';
        return { ok: false, error: this.error || 'Unknown error' };
      } finally {
        this.loading = false;
      }
    },

    async register(data: any): Promise<Result<User | null, string>> {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        this.user = await authService.register(data);
        this.token = await authService.getAccessToken();
        if (!this.user) {
          this.error = 'Registration failed.';
        return { ok: false, error: this.error || 'Unknown error' };
        }
        return { ok: true, value: this.user };
      } catch (err: any) {
        this.error = err.message || 'Registration failed.';
        console.error(err);
        return { ok: false, error: this.error || 'Unknown error' };
      } finally {
        this.loading = false;
      }
    },
  },
});
