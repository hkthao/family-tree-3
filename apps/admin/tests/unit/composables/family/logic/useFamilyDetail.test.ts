import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useFamilyDetail } from '@/composables/family/logic/useFamilyDetail';
import { ref } from 'vue';
import type { FamilyDto } from '@/types';

// Mock dependencies
const mockT = vi.fn((key: string) => key);
const mockUseI18n = vi.fn(() => ({ t: mockT }));

const mockPush = vi.fn();
const mockUseRouter = vi.fn(() => ({ push: mockPush }));

const mockIsAdmin = ref(false);
const mockIsFamilyManagerFn: Mock<((familyId: string) => boolean)> = vi.fn(() => false);
const mockIsFamilyManager = ref<((familyId: string) => boolean)>(mockIsFamilyManagerFn);
const mockUseAuth = vi.fn(() => ({
  state: {
    isAdmin: mockIsAdmin,
    isFamilyManager: mockIsFamilyManager,
  },
}));

const mockFamily = ref<FamilyDto | null>(null);
const mockIsLoading = ref(false);
const mockError = ref<Error | null>(null);
const mockUseFamilyQuery = vi.fn(() => ({
  family: mockFamily,
  isLoading: mockIsLoading,
  error: mockError,
}));

describe('useFamilyDetail', () => {
  let emit: (event: 'openEditDrawer', familyId: string) => void;
  let deps: any;
  const mockFamilyId = 'family123';
  const mockReadOnly = true;

  beforeEach(() => {
    vi.clearAllMocks();
    emit = vi.fn();
    mockIsAdmin.value = false;
    mockIsFamilyManagerFn.mockReturnValue(false); // Corrected
    mockFamily.value = { id: mockFamilyId, name: 'Test FamilyDto', managerIds: [], viewerIds: [] };
    mockIsLoading.value = false;
    mockError.value = null;

    deps = {
      useI18n: mockUseI18n,
      useRouter: mockUseRouter,
      useAuth: mockUseAuth,
      useFamilyQuery: mockUseFamilyQuery,
    };
  });

  it('should initialize with family data, loading state, and error from useFamilyQuery', () => {
    mockIsLoading.value = true;
    mockError.value = new Error('Test Error');
    const { state: { familyData, isLoading, error } } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);

    expect(mockUseFamilyQuery).toHaveBeenCalledWith(expect.any(Object));
    expect(familyData.value).toEqual({ id: mockFamilyId, name: 'Test FamilyDto', managerIds: [], viewerIds: [] });
    expect(isLoading.value).toBe(true);
    expect(error.value).toEqual(new Error('Test Error'));
  });

  it('should return canManageFamily as true if user is admin', () => {
    mockIsAdmin.value = true;
    const { state: { canManageFamily } } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    expect(canManageFamily.value).toBe(true);
  });

  it('should return canManageFamily as true if user is family manager for the current family', () => {
    mockIsFamilyManagerFn.mockImplementation((id: string): boolean => id === mockFamilyId);
    const { state: { canManageFamily } } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    expect(canManageFamily.value).toBe(true);
  });

  it('should return canManageFamily as false if user is not admin or family manager', () => {
    const { state: { canManageFamily } } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    expect(canManageFamily.value).toBe(false);
  });

  it('should emit "openEditDrawer" with familyId when openEditDrawer is called', () => {
    const { actions } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    actions.openEditDrawer();
    expect(emit).toHaveBeenCalledWith('openEditDrawer', mockFamilyId);
  });

  it('should navigate to /family when closeView is called', () => {
    const { actions } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    actions.closeView();
    expect(mockPush).toHaveBeenCalledWith('/family');
  });
});