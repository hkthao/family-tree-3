import { describe, it, expect, vi, beforeEach } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import { ref, type Ref, computed } from 'vue';
import { QueryClient, QueryClientProvider } from '@tanstack/vue-query';
import { useMemoryItemAdd } from '@/composables/memory-item/useMemoryItemAdd';
import type { MemoryItem, FamilyMedia } from '@/types';
import type { MemoryItemFormExpose } from '@/components/memory-item/MemoryItemForm.vue';

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

const mockAddMemoryItemMutate = vi.fn();
const mockAddMemoryItemIsPending = ref(false);
vi.mock('@/composables/memory-item', () => ({
  useAddMemoryItemMutation: vi.fn(() => ({
    mutate: mockAddMemoryItemMutate,
    isPending: mockAddMemoryItemIsPending,
  })),
}));

const mockAddFamilyMediaMutateAsync = vi.fn();
vi.mock('@/composables/family-media/useFamilyMediaMutations', () => ({
  useAddFamilyMediaMutation: vi.fn(() => ({
    mutateAsync: mockAddFamilyMediaMutateAsync,
  })),
}));

// Helper to mount a component that uses the composable
const queryClient = new QueryClient();

function mountComposable(
  familyId: string = 'test-family-id',
  onSaveSuccess = vi.fn(),
  onCancel = vi.fn(),
  formRef: Ref<MemoryItemFormExpose | null> = ref(null),
) {
  let composable: ReturnType<typeof useMemoryItemAdd>;
  shallowMount({
    setup() {
      composable = useMemoryItemAdd({ familyId, onSaveSuccess, onCancel, formRef });
      return () => {};
    },
  }, {
    global: {
      plugins: [[QueryClientProvider, { client: queryClient }]],
    },
  });
  return composable!;
}

describe('useMemoryItemAdd', () => {
  let mockFormRef: Ref<MemoryItemFormExpose | null>;
  let mockOnSaveSuccess: ReturnType<typeof vi.fn>;
  let mockOnCancel: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    vi.clearAllMocks();
    mockFormRef = ref<MemoryItemFormExpose | null>({
      validate: vi.fn(() => Promise.resolve(true)),
      getFormData: vi.fn(() => ({
        familyId: 'test-family-id',
        title: 'Test Memory',
      } as Omit<MemoryItem, 'id'>)),
      newlyUploadedFiles: computed(() => []), // Mock as ComputedRef
    });
    mockOnSaveSuccess = vi.fn();
    mockOnCancel = vi.fn();
    mockAddMemoryItemIsPending.value = false;
  });

  it('should initialize with default values', () => {
    const { isAddingMemoryItem, isUploadingMedia } = mountComposable();

    expect(isAddingMemoryItem.value).toBe(false);
    expect(isUploadingMedia.value).toBe(false);
  });

  it('handleAddItem should return early if formRef is null', async () => {
    const { handleAddItem } = mountComposable('test-family-id', mockOnSaveSuccess, mockOnCancel, ref(null));

    await handleAddItem();

    expect(mockFormRef.value?.validate).not.toHaveBeenCalled();
    expect(mockAddMemoryItemMutate).not.toHaveBeenCalled();
    expect(mockShowSnackbar).not.toHaveBeenCalled();
  });

  it('handleAddItem should return early if form validation fails', async () => {
    mockFormRef.value!.validate.mockResolvedValueOnce(false);
    const { handleAddItem } = mountComposable('test-family-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleAddItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).not.toHaveBeenCalled();
    expect(mockAddMemoryItemMutate).not.toHaveBeenCalled();
    expect(mockShowSnackbar).not.toHaveBeenCalled();
  });

  it('handleAddItem should add memory item without media if no new files', async () => {
    mockAddMemoryItemMutate.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onSuccess();
    });
    const { handleAddItem } = mountComposable('test-family-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleAddItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).toHaveBeenCalledOnce();
    expect(mockAddFamilyMediaMutateAsync).not.toHaveBeenCalled();
    expect(mockAddMemoryItemMutate).toHaveBeenCalledOnce();
    expect(mockAddMemoryItemMutate).toHaveBeenCalledWith(
      { familyId: 'test-family-id', title: 'Test Memory', memoryMedia: [] },
      expect.any(Object)
    );
    expect(mockShowSnackbar).toHaveBeenCalledWith('memoryItem.messages.addSuccess', 'success');
    expect(mockOnSaveSuccess).toHaveBeenCalledOnce();
  });

  it('handleAddItem should upload media and then add memory item', async () => {
    const mockFile = new File(['dummy content'], 'test.png', { type: 'image/png' });
    mockFormRef.value!.newlyUploadedFiles.value = [mockFile];
    const mockUploadedMedia: FamilyMedia = { id: 'media-id-1', filePath: 'path/to/image.png' } as FamilyMedia;

    mockAddFamilyMediaMutateAsync.mockResolvedValueOnce(mockUploadedMedia);
    mockAddMemoryItemMutate.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onSuccess();
    });

    const { handleAddItem, isUploadingMedia } = mountComposable('test-family-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleAddItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).toHaveBeenCalledOnce();
    expect(isUploadingMedia.value).toBe(false); // Should be false after upload is complete
    expect(mockAddFamilyMediaMutateAsync).toHaveBeenCalledOnce();
    expect(mockAddFamilyMediaMutateAsync).toHaveBeenCalledWith({ familyId: 'test-family-id', file: mockFile });
    expect(mockShowSnackbar).toHaveBeenCalledWith('familyMedia.messages.uploadSuccess', 'success');

    expect(mockAddMemoryItemMutate).toHaveBeenCalledOnce();
    expect(mockAddMemoryItemMutate).toHaveBeenCalledWith(
      {
        familyId: 'test-family-id',
        title: 'Test Memory',
        memoryMedia: [{ id: 'media-id-1', memoryItemId: '', url: 'path/to/image.png' }],
      },
      expect.any(Object)
    );
    expect(mockShowSnackbar).toHaveBeenCalledWith('memoryItem.messages.addSuccess', 'success');
    expect(mockOnSaveSuccess).toHaveBeenCalledOnce();
  });

  it('handleAddItem should handle media upload failure', async () => {
    const mockFile = new File(['dummy content'], 'test.png', { type: 'image/png' });
    mockFormRef.value!.newlyUploadedFiles.value = [mockFile];
    const mockMediaUploadError = new Error('Media upload failed');

    mockAddFamilyMediaMutateAsync.mockRejectedValueOnce(mockMediaUploadError);

    const { handleAddItem, isUploadingMedia } = mountComposable('test-family-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleAddItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).toHaveBeenCalledOnce();
    expect(isUploadingMedia.value).toBe(false); // Should be false after upload attempt
    expect(mockAddFamilyMediaMutateAsync).toHaveBeenCalledOnce();
    expect(mockAddFamilyMediaMutateAsync).toHaveBeenCalledWith({ familyId: 'test-family-id', file: mockFile });
    expect(mockShowSnackbar).toHaveBeenCalledWith(mockMediaUploadError.message, 'error');
    expect(mockAddMemoryItemMutate).not.toHaveBeenCalled(); // Memory item should not be added
    expect(mockOnSaveSuccess).not.toHaveBeenCalled();
  });

  it('handleAddItem should handle memory item add failure', async () => {
    mockAddMemoryItemMutate.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onError(new Error('Memory item save failed'));
    });
    const { handleAddItem } = mountComposable('test-family-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    await handleAddItem();

    expect(mockFormRef.value?.validate).toHaveBeenCalledOnce();
    expect(mockFormRef.value?.getFormData).toHaveBeenCalledOnce();
    expect(mockAddFamilyMediaMutateAsync).not.toHaveBeenCalled();
    expect(mockAddMemoryItemMutate).toHaveBeenCalledOnce();
    expect(mockShowSnackbar).toHaveBeenCalledWith('Memory item save failed', 'error');
    expect(mockOnSaveSuccess).not.toHaveBeenCalled();
  });

  it('closeForm should call the onCancel callback', () => {
    const { closeForm } = mountComposable('test-family-id', mockOnSaveSuccess, mockOnCancel, mockFormRef);

    closeForm();

    expect(mockOnCancel).toHaveBeenCalledOnce();
  });
});
