import { setActivePinia, createPinia } from 'pinia';
import { useFamilyDataStore } from '@/stores/family-data.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { FamilyExportDto } from '@/types/family';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';
// import i18n from '@/plugins/i18n'; // REMOVED

// Mock the IFamilyDataService
const mockExportFamilyData = vi.fn();
const mockImportFamilyData = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    familyData: {
      exportFamilyData: mockExportFamilyData,
      importFamilyData: mockImportFamilyData,
    },
    // Add other services as empty objects
    ai: {}, auth: {}, chat: {}, event: {}, face: {}, family: {},
    member: {}, naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {},
    userSettings: {}, familyDict: {},
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key),
    },
  },
}));

describe('family-data.store', () => {
  let store: ReturnType<typeof useFamilyDataStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFamilyDataStore();
    store.$reset();
    store.services = createServices('test');
    mockExportFamilyData.mockReset();
    mockImportFamilyData.mockReset();

    // Default mock resolved values
    mockExportFamilyData.mockResolvedValue(ok(mockFamilyExportDto));
    mockImportFamilyData.mockResolvedValue(ok('new-family-id'));

    // Mock DOM elements and functions for download
    const mockAnchor = {
      href: '',
      download: '',
      click: vi.fn(),
    };
    vi.spyOn(document, 'createElement').mockReturnValue(mockAnchor as any);
    vi.spyOn(document.body, 'appendChild').mockImplementation(vi.fn());
    vi.spyOn(document.body, 'removeChild').mockImplementation(vi.fn());
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock-url');
    // Ensure revokeObjectURL exists before spying on it
    if (!global.URL.revokeObjectURL) {
      global.URL.revokeObjectURL = vi.fn();
    }
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(vi.fn());

    // Mock i18n
    vi.mock('@/plugins/i18n', () => ({
      default: {
        global: {
          t: vi.fn((key) => key), // Mock the translation function to return the key itself
        },
      },
    }));
  });

  const mockFamilyExportDto: FamilyExportDto = {
    id: 'family-1',
    name: 'Test Family',
    description: 'A test family',
    address: 'Test Address',
    avatarUrl: 'test-avatar.jpg',
    visibility: 0,
    familyUsers: [],
    members: [],
    relationships: [],
    events: [],
    settings: { id: 'setting-1', familyId: 'family-1', jsonConfig: '{}' },
    privacyConfiguration: { id: 'privacy-1', familyId: 'family-1', jsonConfig: '{}' },
  };

  describe('exportFamilyData', () => {
    it('should export data successfully and trigger download', async () => {
      mockExportFamilyData.mockResolvedValue(ok(mockFamilyExportDto));
      // document.createElement('a') will return the mocked anchor from beforeEach
      const anchorInstance = document.createElement('a') as HTMLAnchorElement;

      const result = await store.exportFamilyData('family-1');

      expect(result).toBe(true);
      expect(store.exporting).toBe(false);
      expect(store.error).toBeNull();
      expect(mockExportFamilyData).toHaveBeenCalledTimes(1);
      expect(mockExportFamilyData).toHaveBeenCalledWith('family-1');
      expect(document.createElement).toHaveBeenCalledWith('a');
      expect(URL.createObjectURL).toHaveBeenCalled();
      expect(document.body.appendChild).toHaveBeenCalledWith(anchorInstance);
      expect(anchorInstance.click).toHaveBeenCalled(); // Check on the mocked element
      expect(document.body.removeChild).toHaveBeenCalledWith(anchorInstance);
      expect(URL.revokeObjectURL).toHaveBeenCalled();
    });

    it('should handle export failure', async () => {
      const errorMessage = 'Failed to export family data.';
      mockExportFamilyData.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.exportFamilyData('family-1');

      expect(result).toBe(false);
      expect(store.exporting).toBe(false);
      expect(store.error).toBe(errorMessage); // Expect the error message directly
      expect(mockExportFamilyData).toHaveBeenCalledTimes(1);
      expect(URL.createObjectURL).not.toHaveBeenCalled();
    });

    it('should set loading state correctly', async () => {
      mockExportFamilyData.mockResolvedValue(ok(mockFamilyExportDto));
      const promise = store.exportFamilyData('family-1');

      expect(store.exporting).toBe(true);
      await promise;
      expect(store.exporting).toBe(false);
    });
  });

  describe('importFamilyData', () => {
    it('should import data successfully', async () => {
      mockImportFamilyData.mockResolvedValue(ok('new-family-id'));

      const result = await store.importFamilyData('family-1', mockFamilyExportDto, true);

      expect(result).toBe('new-family-id');
      expect(store.importing).toBe(false);
      expect(store.error).toBeNull();
      expect(mockImportFamilyData).toHaveBeenCalledTimes(1);
      expect(mockImportFamilyData).toHaveBeenCalledWith('family-1', mockFamilyExportDto, true);
    });

    it('should handle import failure', async () => {
      const errorMessage = 'Failed to import family data.';
      mockImportFamilyData.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.importFamilyData('family-1', mockFamilyExportDto, true);

      expect(result).toBeNull();
      expect(store.importing).toBe(false);
      expect(store.error).toBe(errorMessage); // Expect the error message directly
      expect(mockImportFamilyData).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockImportFamilyData.mockResolvedValue(ok('new-family-id'));
      const promise = store.importFamilyData('family-1', mockFamilyExportDto, true);

      expect(store.importing).toBe(true);
      await promise;
      expect(store.importing).toBe(false);
    });
  });
});
