import { describe, it, expect, vi, beforeEach } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import { QueryClient, QueryClientProvider } from '@tanstack/vue-query';
import { useMemoryItemEdit } from '@/composables/memory-item/useMemoryItemEdit';
import type { MemoryItem } from '@/types';
import type { MemoryItemFormExpose } from '@/components/memory-item/MemoryItemForm.vue';
import { ref as vueRef, computed } from 'vue';

// Mock dependencies
const mockT = vi.fn((key: string) => key);
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(() => ({
    t: mockT,
  })),
}));

const mockShowSnackbar = vi.fn();
vi.mock('@/composables', () => ({
  useGlobalSnackbar: vi.fn(() => ({
    showSnackbar: mockShowSnackbar,
  })),
}));

// Mock functions from @/composables/memory-item
vi.mock('@/composables/memory-item', () => ({
  useMemoryItemQuery: vi.fn(),
  useUpdateMemoryItemMutation: vi.fn(),
}));

let mockUseMemoryItemQuerySpy: ReturnType<typeof vi.fn>;
let mockUseUpdateMemoryItemMutationSpy: ReturnType<typeof vi.fn>;
let mockUpdateMemoryItemMutateSpy: ReturnType<typeof vi.fn>;
let mockUpdateMemoryItemIsPending: ReturnType<typeof vueRef>;
let mockQueryReturnValue: ReturnType<typeof vueRef>;

const queryClient = new QueryClient();

function mountComposable(
  familyId: string = 'test-family-id',
  memoryItemId: string = 'test-memory-item-id',
  onSaveSuccess = vi.fn(),
  onCancel = vi.fn(),
  formRef: Ref<MemoryItemFormExpose | null> = vueRef(null),
) {
  let composable: ReturnType<typeof useMemoryItemEdit>;
  shallowMount({
    setup() {
      composable = useMemoryItemEdit({ familyId, memoryItemId, onSaveSuccess, onCancel, formRef });
      return () => {};
    },
  }, {
    global: {
      plugins: [[QueryClientProvider, { client: queryClient }]],
    },
  });
  return composable!;
}

describe('useMemoryItemEdit', () => {
  let mockFormRef: Ref<MemoryItemFormExpose | null>;
  let mockOnSaveSuccess: ReturnType<typeof vi.fn>;
  let mockOnCancel: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    vi.clearAllMocks();

    mockFormRef = vueRef<MemoryItemFormExpose | null>({
      validate: vi.fn(() => Promise.resolve(true)),
      getFormData: vi.fn(() => ({
        id: 'test-memory-item-id',
        familyId: 'test-family-id',
        title: 'Updated Memory',
      } as MemoryItem)),
      newlyUploadedFiles: computed(() => []),
    });
    mockOnSaveSuccess = vi.fn();
    mockOnCancel = vi.fn();

    const memoryItemModule = await import('@/composables/memory-item');

    mockUseMemoryItemQuerySpy = vi.spyOn(memoryItemModule, 'useMemoryItemQuery');
    mockUseUpdateMemoryItemMutationSpy = vi.spyOn(memoryItemModule, 'useUpdateMemoryItemMutation');

    mockQueryReturnValue = vueRef({ data: null, isLoading: false, error: null });
    mockUpdateMemoryItemIsPending = vueRef(false);
    mockUpdateMemoryItemMutateSpy = vi.fn();

    mockUseMemoryItemQuerySpy.mockImplementation(() => {
      return mockQueryReturnValue.value;
    });

    mockUseUpdateMemoryItemMutationSpy.mockImplementation(() => ({
      mutate: mockUpdateMemoryItemMutateSpy,
      isPending: mockUpdateMemoryItemIsPending,
    }));
  });

  it('should initialize with default values and fetch memory item', () => {
    const mockMemoryItem: MemoryItem = {
      id: 'test-memory-item-id',
      familyId: 'test-family-id',
      title: 'Initial Memory',
      emotionalTag: 0,
      memoryMedia: [],
      memoryPersons: [],
      personIds: [],
    };
    mockQueryReturnValue.value = {
      data: vueRef(mockMemoryItem),
      isLoading: vueRef(false),
      error: vueRef(null),
    };

    const { memoryItem, isLoading, isUpdatingMemoryItem } = mountComposable('test-family-id', 'test-memory-item-id');

    expect(mockUseMemoryItemQuerySpy).toHaveBeenCalledWith('test-family-id', 'test-memory-item-id');
    expect(memoryItem.value).toEqual(mockMemoryItem);
    expect(isLoading.value).toBe(false);
    expect(isUpdatingMemoryItem.value).toBe(false);
  });

  it('handleUpdateItem should return early if formRef is null', async () => {
    const { handleUpdateItem } = mountComposable('test-family-id', 'test-memory-item-id', mockOnSaveSuccess, mockOnCancel, vueRef(null));

    await handleUpdateItem();

    expect(mockFormRef.value?.validate).not.toHaveBeenCalled();
    expect(mockUpdateMemoryItemMutateSpy).not.toHaveBeenCalled();
    expect(mockShowSnackbar).not.toHaveBeenCalled();
  });

  it('handleUpdateItem should return early if form validation fails', async () => {
    mockFormRef.value!.validate.mockResolvedValueOnce(false);
    const { handleUpdateItem } = mountComposable('test-family-id', 'test-memory-item-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleUpdateItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).not.toHaveBeenCalled();
    expect(mockUpdateMemoryItemMutateSpy).not.toHaveBeenCalled();
    expect(mockShowSnackbar).not.toHaveBeenCalled();
  });

  it('handleUpdateItem should show error if itemData has no ID', async () => {
    mockFormRef.value!.getFormData.mockReturnValueOnce({
      familyId: 'test-family-id',
      title: 'Updated Memory',
    } as MemoryItem); // Missing ID

    const { handleUpdateItem } = mountComposable('test-family-id', 'test-memory-item-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleUpdateItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).toHaveBeenCalledOnce();
    expect(mockUpdateMemoryItemMutateSpy).not.toHaveBeenCalled();
    expect(mockShowSnackbar).toHaveBeenCalledWith('memoryItem.messages.saveError', 'error');
    expect(mockOnSaveSuccess).not.toHaveBeenCalled();
  });

  it('handleUpdateItem should update memory item on success', async () => {
    mockUpdateMemoryItemMutateSpy.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onSuccess();
    });

    const { handleUpdateItem } = mountComposable('test-family-id', 'test-memory-item-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleUpdateItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).toHaveBeenCalledOnce();
    expect(mockUpdateMemoryItemMutateSpy).toHaveBeenCalledOnce();
    expect(mockUpdateMemoryItemMutateSpy).toHaveBeenCalledWith(
      { id: 'test-memory-item-id', familyId: 'test-family-id', title: 'Updated Memory' },
      expect.any(Object)
    );
    expect(mockShowSnackbar).toHaveBeenCalledWith('memoryItem.messages.updateSuccess', 'success');
    expect(mockOnSaveSuccess).toHaveBeenCalledOnce();
  });

  it('handleUpdateItem should show error on update failure', async () => {
    const mockError = new Error('Update failed');
    mockUpdateMemoryItemMutateSpy.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onError(mockError);
    });

    const { handleUpdateItem } = mountComposable('test-family-id', 'test-memory-item-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleUpdateItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).toHaveBeenCalledOnce();
    expect(mockUpdateMemoryItemMutateSpy).toHaveBeenCalledOnce();
    expect(mockUpdateMemoryItemMutateSpy).toHaveBeenCalledWith(
      { id: 'test-memory-item-id', familyId: 'test-family-id', title: 'Updated Memory' },
      expect.any(Object)
    );
    expect(mockShowSnackbar).toHaveBeenCalledWith(mockError.message, 'error');
    expect(mockOnSaveSuccess).not.toHaveBeenCalled();
  });

  it('closeForm should call the onCancel callback', () => {
    const { closeForm } = mountComposable('test-family-id', 'test-memory-item-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    closeForm();

    expect(mockOnCancel).toHaveBeenCalledOnce();
  });
});