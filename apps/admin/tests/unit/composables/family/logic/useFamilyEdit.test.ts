import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useFamilyEdit } from '@/composables/family/logic/useFamilyEdit';
import { ref, type Ref } from 'vue';

// Mock dependencies
const mockT = vi.fn((key: string) => key);
const mockUseI18n = vi.fn(() => ({ t: mockT }));

const mockShowSnackbar = vi.fn();
const mockUseGlobalSnackbar = vi.fn(() => ({ showSnackbar: mockShowSnackbar }));

const mockFamily = ref({ id: 'family1', name: 'Test Family' });
const mockIsLoadingFamily = ref(false);
const mockErrorFamily = ref<Error | null>(null);
const mockUseFamilyQuery = vi.fn(() => ({
  family: mockFamily,
  isLoading: mockIsLoadingFamily,
  error: mockErrorFamily,
}));

const mockUpdateFamily = vi.fn();
const mockIsUpdatingFamily = ref(false);
const mockUseUpdateFamilyMutation = vi.fn(() => ({
  mutate: mockUpdateFamily,
  isPending: mockIsUpdatingFamily,
}));

const mockFamilyFormRef: Ref<any | null> = ref({
  validate: vi.fn(() => true),
  getFormData: vi.fn(() => ({ id: 'family1', name: 'Updated Family' })),
});

describe('useFamilyEdit', () => {
  let emit: (event: 'close' | 'saved') => void;
  let deps: any;
  const mockProps = { familyId: 'family1' };

  beforeEach(() => {
    vi.clearAllMocks();
    emit = vi.fn();
    mockFamily.value = { id: 'family1', name: 'Test Family' };
    mockIsLoadingFamily.value = false;
    mockErrorFamily.value = null;
    mockIsUpdatingFamily.value = false;
    mockFamilyFormRef.value.validate.mockResolvedValue(true);
    mockFamilyFormRef.value.getFormData.mockReturnValue({ id: 'family1', name: 'Updated Family' });

    deps = {
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useFamilyQuery: mockUseFamilyQuery,
      useUpdateFamilyMutation: mockUseUpdateFamilyMutation,
    };
  });

  it('should initialize with family data, loading state, and error from useFamilyQuery', () => {
    mockIsLoadingFamily.value = true;
    mockErrorFamily.value = new Error('Test Error');
    const { state } = useFamilyEdit(mockProps, emit, mockFamilyFormRef, deps);

    expect(mockUseFamilyQuery).toHaveBeenCalledWith(expect.any(Object)); // Expecting toRef result
    expect(state.family.value).toEqual({ id: 'family1', name: 'Test Family' });
    expect(state.isLoading.value).toBe(true);
    expect(state.error.value).toEqual(new Error('Test Error'));
    expect(state.isUpdatingFamily.value).toBe(false);
  });

  it('should call familyFormRef.value.validate and getFormData when handleUpdateItem is called', async () => {
    const { actions } = useFamilyEdit(mockProps, emit, mockFamilyFormRef, deps);

    await actions.handleUpdateItem();

    expect(mockFamilyFormRef.value.validate).toHaveBeenCalledOnce();
    expect(mockFamilyFormRef.value.getFormData).toHaveBeenCalledOnce();
  });

  it('should not call updateFamily if validation fails', async () => {
    mockFamilyFormRef.value.validate.mockResolvedValueOnce(false);
    const { actions } = useFamilyEdit(mockProps, emit, mockFamilyFormRef, deps);

    await actions.handleUpdateItem();

    expect(mockFamilyFormRef.value.validate).toHaveBeenCalledOnce();
    expect(mockFamilyFormRef.value.getFormData).not.toHaveBeenCalled();
    expect(mockUpdateFamily).not.toHaveBeenCalled();
  });

  it('should call updateFamily with correct data on successful validation', async () => {
    const mockFormData = { id: 'family1', name: 'New Updated Family' };
    mockFamilyFormRef.value.getFormData.mockReturnValueOnce(mockFormData);
    const { actions } = useFamilyEdit(mockProps, emit, mockFamilyFormRef, deps);

    await actions.handleUpdateItem();

    expect(mockUpdateFamily).toHaveBeenCalledWith(mockFormData, expect.any(Object));
  });

  it('should call showSnackbar with success message and emit "saved" on successful mutation', async () => {
    const { actions } = useFamilyEdit(mockProps, emit, mockFamilyFormRef, deps);

    mockUpdateFamily.mockImplementation((_data, callbacks) => {
      callbacks.onSuccess();
    });

    await actions.handleUpdateItem();

    expect(mockShowSnackbar).toHaveBeenCalledWith('family.management.messages.updateSuccess', 'success');
    expect(emit).toHaveBeenCalledWith('saved');
  });

  it('should call showSnackbar with error message on failed mutation', async () => {
    const mockError = new Error('Update failed');
    const { actions } = useFamilyEdit(mockProps, emit, mockFamilyFormRef, deps);

    mockUpdateFamily.mockImplementation((_data, callbacks) => {
      callbacks.onError(mockError);
    });

    await actions.handleUpdateItem();

    expect(mockShowSnackbar).toHaveBeenCalledWith(mockError.message, 'error');
    expect(emit).not.toHaveBeenCalledWith('saved');
  });

  it('should emit "close" when closeForm is called', () => {
    const { actions } = useFamilyEdit(mockProps, emit, mockFamilyFormRef, deps);

    actions.closeForm();

    expect(emit).toHaveBeenCalledWith('close');
  });
});
