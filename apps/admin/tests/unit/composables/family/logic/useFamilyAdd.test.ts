import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useFamilyAdd } from '@/composables/family/logic/useFamilyAdd';
import { ref } from 'vue';

// Mock dependencies
const mockT = vi.fn((key: string) => key);
const mockUseI18n = vi.fn(() => ({ t: mockT }));

const mockShowSnackbar = vi.fn();
const mockUseGlobalSnackbar = vi.fn(() => ({ showSnackbar: mockShowSnackbar }));

const mockAddFamily = vi.fn();
const mockIsAddingFamily = ref(false);
const mockUseAddFamilyMutation = vi.fn(() => ({
  mutate: mockAddFamily,
  isPending: mockIsAddingFamily,
}));

describe('useFamilyAdd', () => {
  let emit: (event: 'close' | 'saved') => void;
  let deps: any;
  let mockFamilyFormRef: any;

  beforeEach(() => {
    vi.clearAllMocks();
    emit = vi.fn();
    mockFamilyFormRef = ref(null);
    mockFamilyFormRef.value = {
      validate: vi.fn(() => true),
      getFormData: vi.fn(() => ({ name: 'Test FamilyDto' })),
    };
    deps = {
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useAddFamilyMutation: mockUseAddFamilyMutation,
      familyFormRef: mockFamilyFormRef,
    };
    mockIsAddingFamily.value = false;
  });

  it('should call familyFormRef.value.validate and getFormData when handleAddItem is called', async () => {
    const { actions } = useFamilyAdd(emit, mockFamilyFormRef, deps);

    await actions.handleAddItem();

    expect(mockFamilyFormRef.value.validate).toHaveBeenCalledOnce();
    expect(mockFamilyFormRef.value.getFormData).toHaveBeenCalledOnce();
  });

  it('should not call addFamily if validation fails', async () => {
    mockFamilyFormRef.value.validate.mockResolvedValueOnce(false);
    const { actions } = useFamilyAdd(emit, mockFamilyFormRef, deps);

    await actions.handleAddItem();

    expect(mockFamilyFormRef.value.validate).toHaveBeenCalledOnce();
    expect(mockFamilyFormRef.value.getFormData).not.toHaveBeenCalled();
    expect(mockAddFamily).not.toHaveBeenCalled();
  });

  it('should call addFamily with correct data on successful validation', async () => {
    const mockFormData = { name: 'New FamilyDto' };
    mockFamilyFormRef.value.getFormData.mockReturnValueOnce(mockFormData);
    const { actions } = useFamilyAdd(emit, mockFamilyFormRef, deps);

    await actions.handleAddItem();

    expect(mockAddFamily).toHaveBeenCalledWith(mockFormData, expect.any(Object));
  });

  it('should call showSnackbar with success message and emit "close" on successful mutation', async () => {
    const { actions } = useFamilyAdd(emit, mockFamilyFormRef, deps);

    mockAddFamily.mockImplementation((_data, callbacks) => {
      callbacks.onSuccess();
    });

    await actions.handleAddItem();

    expect(mockShowSnackbar).toHaveBeenCalledWith('family.management.messages.addSuccess', 'success');
    expect(emit).toHaveBeenCalledWith('close');
  });

  it('should call showSnackbar with error message on failed mutation', async () => {
    const mockError = new Error('Mutation failed');
    const { actions } = useFamilyAdd(emit, mockFamilyFormRef, deps);

    mockAddFamily.mockImplementation((_data, callbacks) => {
      callbacks.onError(mockError);
    });

    await actions.handleAddItem();

    expect(mockShowSnackbar).toHaveBeenCalledWith(mockError.message, 'error');
    expect(emit).not.toHaveBeenCalledWith('close');
  });

  it('should emit "close" when closeForm is called', () => {
    const { actions } = useFamilyAdd(emit, mockFamilyFormRef, deps);

    actions.closeForm();

    expect(emit).toHaveBeenCalledWith('close');
  });
});