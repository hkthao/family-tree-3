import { defineStore } from 'pinia';
import { useAuthService } from '@/services/auth/authService';
import type { AuthUser } from '@/services/auth/authService';
import type { Credentials } from '@/types/auth';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as AuthUser | null,
    token: null as string | null,
    loading: false,
    error: null as string | null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.user,
    isAdmin: (state) => state.user?.roles?.includes('Admin'),
    isFamilyManager: (state) => state.user?.roles?.includes('FamilyManager'),
  },
  actions: {
    async initAuth() {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        this.user = await authService.getUser();
        this.token = await authService.getAccessToken();
      } catch (err: any) {
        this.error = err.message || 'Failed to initialize authentication.';
        console.error(err);
      } finally {
        this.loading = false;
      }
    },

    async login(credentials: Credentials) {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        this.user = await authService.login(credentials);
        this.token = await authService.getAccessToken();
        if (!this.user) {
          this.error = 'Invalid credentials or login failed.';
        }
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
