import { describe, it, expect, vi, beforeEach } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import { useProfileSettings } from '@/composables/user/useProfileSettings';
import { QueryClient, useQueryClient } from '@tanstack/vue-query';
import { ApiUserService } from '@/services/user/api.user.service';
import { reactive } from 'vue';

// Mock dependencies
vi.mock('vue-i18n', () => ({
  useI18n: () => ({ t: (key: string) => key }),
}));

vi.mock('@/composables', () => ({
  useGlobalSnackbar: () => ({ showSnackbar: vi.fn() }),
}));

vi.mock('@/services/user/api.user.service');

vi.mock('@vuelidate/core', () => ({
  useVuelidate: vi.fn(() => reactive({
    $validate: vi.fn(() => true),
    firstName: { $touch: vi.fn(), $errors: [] },
    lastName: { $touch: vi.fn(), $errors: [] },
    email: { $touch: vi.fn(), $errors: [] },
  })),
}));

// Create a wrapper component to use the composable
const createComposableWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false, // Disable retries for tests
      },
    },
  });
  return shallowMount({
    template: '<div/>',
    setup() {
      // Provide the query client
      useQueryClient(); // This implicitly uses the client provided by the QueryClientProvider
      return {
        ...useProfileSettings(),
      };
    },
  }, {
    global: {
      plugins: [[queryClient, { queryClient }]],
    },
  });
};

describe('useProfileSettings', () => {
  let mockGetCurrentUserProfile: vi.Mock;
  let mockUpdateUserProfile: vi.Mock;
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    mockGetCurrentUserProfile = vi.fn();
    mockUpdateUserProfile = vi.fn();

    // Mock the ApiUserService methods
    (ApiUserService as vi.Mock).mockImplementation(() => ({
      getCurrentUserProfile: mockGetCurrentUserProfile,
      updateUserProfile: mockUpdateUserProfile,
    }));

    queryClient = new QueryClient();
    queryClient.clear(); // Clear the cache before each test
  });

  it('should fetch user profile on setup and initialize formData', async () => {
    const userProfile = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      phone: '123456789',
      avatar: 'avatar.png',
      externalId: 'ext1',
    };
    mockGetCurrentUserProfile.mockResolvedValue({ ok: true, value: userProfile });

    const wrapper = createComposableWrapper();
    await wrapper.vm.$nextTick(); // Wait for any reactive updates

    expect(mockGetCurrentUserProfile).toHaveBeenCalled();
    expect(wrapper.vm.formData.firstName).toBe('John');
    expect(wrapper.vm.formData.lastName).toBe('Doe');
    expect(wrapper.vm.formData.email).toBe('test@example.com');
    expect(wrapper.vm.formData.phone).toBe('123456789');
    expect(wrapper.vm.formData.avatar).toBe('avatar.png');
    expect(wrapper.vm.formData.externalId).toBe('ext1');
  });

  it('should handle error when fetching user profile', async () => {
    mockGetCurrentUserProfile.mockResolvedValue({ ok: false, error: { message: 'Fetch failed' } });
    const showSnackbar = vi.fn();
    vi.mock('@/composables', () => ({
      useGlobalSnackbar: () => ({ showSnackbar }),
    }));

    const wrapper = createComposableWrapper();
    await wrapper.vm.$nextTick();

    expect(mockGetCurrentUserProfile).toHaveBeenCalled();
    expect(showSnackbar).toHaveBeenCalledWith('Fetch failed', 'error');
    expect(wrapper.vm.isFetchError).toBe(true);
  });

  it('should update user profile successfully', async () => {
    const initialProfile = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      phone: '123456789',
      avatar: 'avatar.png',
      externalId: 'ext1',
    };
    const updatedProfileResponse = {
      ...initialProfile,
      firstName: 'Jane',
      avatar: 'new_avatar.png',
    };
    mockGetCurrentUserProfile.mockResolvedValue({ ok: true, value: initialProfile });
    mockUpdateUserProfile.mockResolvedValue({ ok: true, value: updatedProfileResponse });

    const wrapper = createComposableWrapper();
    await wrapper.vm.$nextTick();

    wrapper.vm.formData.firstName = 'Jane';
    await wrapper.vm.saveProfile();

    expect(mockUpdateUserProfile).toHaveBeenCalledWith(expect.objectContaining({
      id: '1',
      firstName: 'Jane',
      name: 'Jane Doe', // generatedFullName
    }));
    expect(wrapper.vm.isSavingProfile).toBe(false);
    // Expect snackbar success message
    const showSnackbar = vi.fn();
    vi.mock('@/composables', () => ({
      useGlobalSnackbar: () => ({ showSnackbar }),
    }));
    expect(showSnackbar).toHaveBeenCalledWith('userSettings.profile.saveSuccess', 'success');
    expect(wrapper.vm.formData.avatar).toBe('new_avatar.png'); // avatar updated locally
    expect(wrapper.vm.formData.avatarBase64).toBeNull(); // base64 cleared
  });

  it('should handle error when updating user profile', async () => {
    const initialProfile = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      phone: '123456789',
      avatar: 'avatar.png',
      externalId: 'ext1',
    };
    mockGetCurrentUserProfile.mockResolvedValue({ ok: true, value: initialProfile });
    mockUpdateUserProfile.mockResolvedValue({ ok: false, error: { message: 'Save failed' } });

    const showSnackbar = vi.fn();
    vi.mock('@/composables', () => ({
      useGlobalSnackbar: () => ({ showSnackbar }),
    }));

    const wrapper = createComposableWrapper();
    await wrapper.vm.$nextTick();

    wrapper.vm.formData.firstName = 'Jane';
    await wrapper.vm.saveProfile();

    expect(mockUpdateUserProfile).toHaveBeenCalled();
    expect(showSnackbar).toHaveBeenCalledWith('Save failed', 'error');
    expect(wrapper.vm.isSavingProfile).toBe(false);
  });

  it('should show validation error if form is invalid', async () => {
    const initialProfile = {
      id: '1',
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      phone: '123456789',
      avatar: 'avatar.png',
      externalId: 'ext1',
    };
    mockGetCurrentUserProfile.mockResolvedValue({ ok: true, value: initialProfile });
    
    // Mock Vuelidate to return false for validation
    (useVuelidate as vi.Mock).mockImplementationOnce(() => reactive({
      $validate: vi.fn(() => false),
      firstName: { $touch: vi.fn(), $errors: [] },
      lastName: { $touch: vi.fn(), $errors: [] },
      email: { $touch: vi.fn(), $errors: [] },
    }));

    const showSnackbar = vi.fn();
    vi.mock('@/composables', () => ({
      useGlobalSnackbar: () => ({ showSnackbar }),
    }));

    const wrapper = createComposableWrapper();
    await wrapper.vm.$nextTick();

    await wrapper.vm.saveProfile();

    expect(wrapper.vm.v$.$validate).toHaveBeenCalled();
    expect(mockUpdateUserProfile).not.toHaveBeenCalled();
    expect(showSnackbar).toHaveBeenCalledWith('userSettings.profile.validationError', 'error');
  });
});