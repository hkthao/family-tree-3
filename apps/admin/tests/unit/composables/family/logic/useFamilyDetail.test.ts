import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useFamilyDetail } from '@/composables/family/logic/useFamilyDetail';
import { ref } from 'vue';

// Mock dependencies
const mockT = vi.fn((key: string) => key);
const mockUseI18n = vi.fn(() => ({ t: mockT }));

const mockPush = vi.fn();
const mockUseRouter = vi.fn(() => ({ push: mockPush }));

const mockIsAdmin = ref(false);
const mockIsFamilyManagerFn = vi.fn(() => false);
const mockIsFamilyManager = ref(mockIsFamilyManagerFn);
const mockUseAuth = vi.fn(() => ({
  state: {
    isAdmin: mockIsAdmin,
    isFamilyManager: mockIsFamilyManager,
  },
}));

const mockFamily = ref(null);
const mockIsLoading = ref(false);
const mockError = ref(null);
const mockUseFamilyQuery = vi.fn(() => ({
  family: mockFamily,
  isLoading: mockIsLoading,
  error: mockError,
}));

describe('useFamilyDetail', () => {
  let emit: (event: 'openEditDrawer', familyId: string) => void;
  let deps;
  const mockFamilyId = 'family123';
  const mockReadOnly = true;

  beforeEach(() => {
    vi.clearAllMocks();
    emit = vi.fn();
    mockIsAdmin.value = false;
    mockIsFamilyManagerFn.mockReturnValue(false); // Corrected
    mockFamily.value = { id: mockFamilyId, name: 'Test Family' };
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
    const { state } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);

    expect(mockUseFamilyQuery).toHaveBeenCalledWith(expect.any(Object));
    expect(state.familyData.value).toEqual({ id: mockFamilyId, name: 'Test Family' });
    expect(state.isLoading.value).toBe(true);
    expect(state.error.value).toEqual(new Error('Test Error'));
  });

  it('should return canManageFamily as true if user is admin', () => {
    mockIsAdmin.value = true;
    const { state } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    expect(state.canManageFamily.value).toBe(true);
  });

  it('should return canManageFamily as true if user is family manager for the current family', () => {
    mockIsFamilyManagerFn.mockImplementation((id: string) => id === mockFamilyId);
    const { state } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    expect(state.canManageFamily.value).toBe(true);
  });

  it('should return canManageFamily as false if user is not admin or family manager', () => {
    const { state } = useFamilyDetail({ familyId: mockFamilyId, readOnly: mockReadOnly }, emit, deps);
    expect(state.canManageFamily.value).toBe(false);
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