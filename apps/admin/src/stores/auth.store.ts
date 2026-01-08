import { defineStore } from 'pinia';
import { useAuthService } from '@/services/auth/authService';
import type { UserProfile, Result, IFamilyAccess } from '@/types'; // Thêm IFamilyAccess vào import

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as UserProfile | null,
    token: null as string | null,
    loading: false,
    error: null as string | null,
    userFamilyAccess: [] as IFamilyAccess[], // Thêm thuộc tính mới để lưu trữ quyền truy cập gia đình
  }),
  getters: {
    isAuthenticated: (state) => !!state.user,
    isAdmin: (state) => state.user?.roles?.includes('Admin'),
    isFamilyManager: (state) => state.user?.roles?.includes('FamilyManager'), // Sẽ được cập nhật sau để kiểm tra theo familyId
    getAccessToken: (state) => state.token,
  },
  actions: {
    // Action để lấy danh sách quyền truy cập gia đình của người dùng
    async fetchUserFamilyAccess() {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.family.getUserFamilyAccess();
        if (result.ok) {
          this.userFamilyAccess = result.value;
        } else {
          this.error = result.error.message || 'Lỗi khi lấy quyền truy cập gia đình của người dùng.';
          console.error('Lỗi khi lấy quyền truy cập gia đình của người dùng:', result.error);
        }
      } catch (err: any) {
        this.error = err.message || 'Lỗi không xác định khi lấy quyền truy cập gia đình của người dùng.';
        console.error('Lỗi không xác định khi lấy quyền truy cập gia đình của người dùng:', err);
      } finally {
        this.loading = false;
      }
    },

    async initAuth(): Promise<Result<UserProfile | null, string>> {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        this.user = await authService.getUser(); // This will be the Auth0 user data
        this.token = await authService.getAccessToken();
        if (this.user) {
          await this.fetchUserFamilyAccess(); // Gọi action mới sau khi người dùng được tải
          await this.services.notification.syncSubscriber(); // Đồng bộ hóa subscriber Novu
        }
        return { ok: true, value: this.user };
      } catch (err: any) {
        this.error = err.message || 'Không thể lấy thông tin người dùng.';
        return { ok: false, error: this.error || 'Lỗi không xác định' };
      } finally {
        this.loading = false;
      }
    },

    async login(): Promise<Result<void, string>> {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        await authService.login({ appState: { target: '/' } });
        return { ok: true, value: undefined };
      } catch (err: any) {
        this.error = err.message || 'Đăng nhập thất bại.';
        console.error(err);
        return { ok: false, error: this.error || 'Lỗi không xác định' };
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
        this.userFamilyAccess = []; // Xóa quyền truy cập gia đình khi đăng xuất
        return { ok: true, value: undefined };
      } catch (err: any) {
        this.error = err.message || 'Đăng xuất thất bại.';
        return { ok: false, error: this.error || 'Lỗi không xác định' };
      } finally {
        this.loading = false;
      }
    },

    async register(data: any): Promise<Result<UserProfile | null, string>> {
      this.loading = true;
      this.error = null;
      try {
        const authService = useAuthService();
        this.user = await authService.register(data);
        this.token = await authService.getAccessToken();
        if (!this.user) {
          this.error = 'Đăng ký thất bại.';
          return { ok: false, error: this.error || 'Lỗi không xác định' };
        }
        await this.fetchUserFamilyAccess(); // Gọi action mới sau khi đăng ký thành công
        return { ok: true, value: this.user };
      } catch (err: any) {
        this.error = err.message || 'Đăng ký thất bại.';
        console.error(err);
        return { ok: false, error: this.error || 'Lỗi không xác định' };
      } finally {
        this.loading = false;
      }
    },
  },
});

