import { describe, it, expect, vi, beforeEach } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import { QueryClient, VUE_QUERY_CLIENT } from '@tanstack/vue-query';
import { useMemoryItemEdit } from '@/composables/memory-item/useMemoryItemEdit';
import type { MemoryItem } from '@/types';
import type { MemoryItemFormExpose } from '@/components/memory-item/MemoryItemForm.vue';
import { ref as vueRef, computed } from 'vue';
import { ServicesInjectionKey } from '@/plugins/services.plugin'; // Import ServicesInjectionKey
import type { IMemoryItemService } from '@/services/memory-item/memory-item.service.interface'; // Import IMemoryItemService

// Mock dependencies
const mockT = vi.fn((key: string) => key);
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(() => ({
    t: mockT,
  })),
  createI18n: vi.fn(() => ({ // Mock createI18n
    install: vi.fn(), // Provide a mock install function
  })),
}));

const mockShowSnackbar = vi.fn();
vi.mock('@/composables', () => ({
  useGlobalSnackbar: vi.fn(() => ({
    showSnackbar: mockShowSnackbar,
  })),
}));

// Mock functions from @/composables/memory-item
const mockUseMemoryItemQuerySpy = vi.fn();
const mockUpdateMemoryItemMutateSpy = vi.fn();
const mockUpdateMemoryItemIsPending = vueRef(false);
const mockQueryReturnValue = vueRef({ data: null, isLoading: false, error: null });

vi.mock('@/composables/memory-item', () => ({
  useMemoryItemQuery: mockUseMemoryItemQuerySpy.mockImplementation(() => mockQueryReturnValue.value),
  useUpdateMemoryItemMutation: vi.fn(() => ({
    mutate: mockUpdateMemoryItemMutateSpy,
    isPending: mockUpdateMemoryItemIsPending,
  })),
}));

const queryClient = new QueryClient();

let mockMemoryItemService: IMemoryItemService; // Declare mockMemoryItemService
let mockAppServices: { memoryItem: IMemoryItemService }; // Declare mockAppServices

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
      // plugins: [[QueryClientProvider, { client: queryClient }]], // Remove QueryClientProvider from plugins
      provide: {
        [ServicesInjectionKey as symbol]: mockAppServices, // Provide mock AppServices
        [VUE_QUERY_CLIENT]: queryClient, // Provide queryClient directly using the correct injection key
      },
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

    // Define mock MemoryItemService
    mockMemoryItemService = {
      getById: vi.fn(),
      search: vi.fn(),
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getByIds: vi.fn(),
    };

    // Define mock AppServices
    mockAppServices = {
      memoryItem: mockMemoryItemService,
      // Add other services if useMemoryItemEdit or its dependencies require them
      // For now, only memoryItem is directly relevant to the current error.
    } as any;

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

    // Clear and reset global mocks
    mockUseMemoryItemQuerySpy.mockClear();
    mockUpdateMemoryItemMutateSpy.mockClear();
    mockUpdateMemoryItemIsPending.value = false;
    mockQueryReturnValue.value = { data: null, isLoading: false, error: null };
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