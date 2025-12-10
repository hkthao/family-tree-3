import { setActivePinia, createPinia } from 'pinia';
import { useAIBiographyStore } from '@/stores/ai-biography.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, BiographyResultDto, Member } from '@/types';
import { BiographyStyle, Gender } from '@/types';
import { ok, err } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock the services
const mockGetById = vi.fn(); // For member service
const mockUpdateMemberBiography = vi.fn(); // For member service
const mockGenerateBiography = vi.fn(); // For aiBiography service
const mockShowSnackbar = vi.fn(); // For notification store

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    member: {
      getById: mockGetById,
      updateMemberBiography: mockUpdateMemberBiography,
    },
        ai: {
          generateBiography: mockGenerateBiography,
        },
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      locale: { value: 'en' },
      t: vi.fn((key) => key),
    },
  },
}));

// Mock notification store
vi.mock('@/stores/notification.store', () => ({
  useNotificationStore: vi.fn(() => ({
    showSnackbar: mockShowSnackbar,
  })),
}));

describe('ai-biography.store', () => {
  let store: ReturnType<typeof useAIBiographyStore>;

  const mockMember: Member = {
    id: 'member-1',
    familyId: 'family-1',
    lastName: 'Doe',
    firstName: 'John',
    fullName: 'John Doe',
    gender: Gender.Female,
    biography: 'Original biography',
  };

  const mockBiographyResult: BiographyResultDto = {
    biography: 'Generated biography content', // Use 'biography' instead of 'content'
  };

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useAIBiographyStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');

    // Reset mocks before each test
    mockGetById.mockReset();
    mockUpdateMemberBiography.mockReset();
    mockGenerateBiography.mockReset();
    mockShowSnackbar.mockReset();

    // Set default mock resolved values
    mockGetById.mockResolvedValue(ok(mockMember));
    mockUpdateMemberBiography.mockResolvedValue(ok(undefined));
    mockGenerateBiography.mockResolvedValue(ok(mockBiographyResult)); // Return the full DTO
  });

  it('should have correct initial state', () => {
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.biographyResult).toBeNull();
    expect(store.memberId).toBeNull();
    expect(store.currentMember).toBeNull();
    expect(store.style).toBe(BiographyStyle.Emotional);
    expect(store.generatedFromDB).toBe(true);
    expect(store.userPrompt).toBeNull();
    expect(store.language).toBe('Vietnamese');
  });

  describe('fetchMemberDetails', () => {
    it('should fetch member details successfully', async () => {
      await store.fetchMemberDetails(mockMember.id!);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.currentMember).toEqual(mockMember);
      // Ensure the biographyResult is also updated correctly
      expect(store.biographyResult?.biography).toEqual(mockMember.biography);
      expect(mockGetById).toHaveBeenCalledTimes(1);
      expect(mockGetById).toHaveBeenCalledWith(mockMember.id);
    });

    it('should handle fetch member details failure', async () => {
      const errorMessage = 'Failed to fetch member.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.fetchMemberDetails(mockMember.id!);

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.currentMember).toBeNull();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });
  });

  describe('generateBiography', () => {
    beforeEach(() => {
      store.memberId = mockMember.id; // Set memberId for generation
    });

    it('should generate biography successfully', async () => {
      await store.generateBiography();

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.biographyResult).toEqual(mockBiographyResult);
      expect(mockGenerateBiography).toHaveBeenCalledTimes(1);
      expect(mockGenerateBiography).toHaveBeenCalledWith(
        store.memberId,
        store.style,
        store.generatedFromDB,
        undefined, // userPrompt is null initially
        store.language, // Add language parameter
      );
    });

    it('should handle biography generation failure', async () => {
      const errorMessage = 'Failed to generate biography.';
      mockGenerateBiography.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.generateBiography();

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.biographyResult).toBeNull();
      expect(mockGenerateBiography).toHaveBeenCalledTimes(1);
    });

    it('should set error if memberId is null', async () => {
      store.memberId = null;
      await store.generateBiography();

      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiBiography.errors.memberIdRequired');
      expect(mockGenerateBiography).not.toHaveBeenCalled();
    });
  });

  describe('saveBiography', () => {
    it('should save biography successfully', async () => {
      const result = await store.saveBiography(mockMember.id!, 'New biography content');

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdateMemberBiography).toHaveBeenCalledTimes(1);
      expect(mockUpdateMemberBiography).toHaveBeenCalledWith(
        mockMember.id,
        'New biography content',
      );
      // Check if currentMember biography is updated
      store.currentMember = { ...mockMember }; // Simulate currentMember being set
      await store.saveBiography(mockMember.id!, 'Another new biography');
      expect(store.currentMember?.biography).toBe('Another new biography');
    });

    it('should handle save biography failure', async () => {
      const errorMessage = 'Failed to save biography.';
      mockUpdateMemberBiography.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.saveBiography(mockMember.id!, 'New biography content');

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(mockUpdateMemberBiography).toHaveBeenCalledTimes(1);
      expect(mockShowSnackbar).not.toHaveBeenCalled();
    });

    it('should set error if memberId or content is empty', async () => {
      let result = await store.saveBiography('', 'content');
      expect(result.ok).toBe(false);
      expect(store.error).toBe('aiBiography.errors.saveFailed');
      expect(mockUpdateMemberBiography).not.toHaveBeenCalled();

      store.$reset(); // Reset store state
      result = await store.saveBiography('member-id', '');
      expect(result.ok).toBe(false);
      expect(store.error).toBe('aiBiography.errors.saveFailed');
      expect(mockUpdateMemberBiography).not.toHaveBeenCalled();
    });
  });

  describe('clearForm', () => {
    it('should clear the form state', () => {
      store.userPrompt = 'some prompt';
      store.biographyResult = mockBiographyResult;
      store.style = BiographyStyle.Emotional;
      store.generatedFromDB = false;

      store.clearForm();

      expect(store.userPrompt).toBeNull();
      expect(store.biographyResult).toBeNull();
      expect(store.style).toBe(BiographyStyle.Emotional);
      expect(store.generatedFromDB).toBe(true);
    });
  });

  it('should set the memberId', () => {
    store.memberId = 'test-member-id';
    expect(store.memberId).toBe('test-member-id');
  });

  it('should set the style', () => {
    store.style = BiographyStyle.Emotional;
    expect(store.style).toBe(BiographyStyle.Emotional);
  });

  it('should set generatedFromDB', () => {
    store.generatedFromDB = false;
    expect(store.generatedFromDB).toBe(false);
  });

  it('should set userPrompt', () => {
    store.userPrompt = 'new prompt';
    expect(store.userPrompt).toBe('new prompt');
  });

  it('should set language', () => {
    store.language = 'English';
    expect(store.language).toBe('English');
  });
});