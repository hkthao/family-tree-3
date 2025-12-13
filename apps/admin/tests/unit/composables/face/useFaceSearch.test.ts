import { describe, it, expect, vi, beforeEach } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import { ref, type Ref, nextTick } from 'vue'; // Import Ref and nextTick from vue
import { ref, type Ref } from 'vue'; // Import Ref
import { useFaceSearch } from '@/composables/face/useFaceSearch';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useDetectFacesMutation } from '@/composables/member-face';
import type { DetectedFace } from '@/types';
import { QueryClient, QueryClientProvider } from '@tanstack/vue-query';

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

const mockMutate = vi.fn();
const mockIsPending = ref(false);
const mockError: Ref<Error | null> = ref(null); // Explicitly type mockError
vi.mock('@/composables/member-face', () => ({
  useDetectFacesMutation: vi.fn(() => ({
    mutate: mockMutate,
    isPending: mockIsPending,
    error: mockError,
  })),
}));

// Helper to mount a component that uses the composable
const queryClient = new QueryClient();

function mountComposable() {
  let composable: ReturnType<typeof useFaceSearch>;
  shallowMount({
    setup() {
      composable = useFaceSearch();
      return () => {};
    },
  }, {
    global: {
      plugins: [[QueryClientProvider, { client: queryClient }]],
    },
  });
  return composable!;
}

describe('useFaceSearch', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockIsPending.value = false;
    mockError.value = null;
  });

  it('should initialize with default values', () => {
    const { selectedFamilyId, uploadedImage, detectedFaces, originalImageUrl, isDetectingFaces } = mountComposable();

    expect(selectedFamilyId.value).toBeUndefined();
    expect(uploadedImage.value).toBeUndefined();
    expect(detectedFaces.value).toEqual([]);
    expect(originalImageUrl.value).toBeNull();
    expect(isDetectingFaces.value).toBe(false);
  });

  it('resetState should clear all relevant refs', () => {
    const { selectedFamilyId, uploadedImage, detectedFaces, originalImageUrl, resetState } = mountComposable();

    selectedFamilyId.value = 'test-family-id';
    uploadedImage.value = 'base64-image';
    detectedFaces.value = [{ id: '1', box: [0, 0, 10, 10] }] as DetectedFace[];
    originalImageUrl.value = 'original-url';

    resetState();

    expect(uploadedImage.value).toBeNull();
    expect(detectedFaces.value).toEqual([]);
    expect(originalImageUrl.value).toBeNull();
    expect(selectedFamilyId.value).toBe('test-family-id'); // selectedFamilyId is not reset by resetState
  });

  it('handleFileUpload should return early if no file is provided', async () => {
    const { handleFileUpload } = mountComposable();

    await handleFileUpload(null);

    expect(mockMutate).not.toHaveBeenCalled();
    expect(mockShowSnackbar).not.toHaveBeenCalled();
  });

  it('handleFileUpload should show warning if no familyId is selected', async () => {
    const { handleFileUpload } = mountComposable();
    const mockFile = new File(['dummy content'], 'test.png', { type: 'image/png' });

    await handleFileUpload(mockFile);

    expect(mockShowSnackbar).toHaveBeenCalledWith('face.selectFamilyToUpload', 'warning');
    expect(mockMutate).not.toHaveBeenCalled();
  });

  it('handleFileUpload should call detectFaces on successful upload', async () => {
    const { selectedFamilyId, uploadedImage, detectedFaces, originalImageUrl, handleFileUpload } = mountComposable();
    selectedFamilyId.value = 'test-family-id';
    const mockFile = new File(['dummy content'], 'test.png', { type: 'image/png' });
    const mockData = {
      originalImageBase64: 'base64-mock',
      detectedFaces: [{ id: 'face1', box: [0, 0, 10, 10] }] as DetectedFace[],
      originalImageUrl: 'original-url-mock',
    };

    // Simulate mutation success
    mockMutate.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onSuccess(mockData);
    });

    await handleFileUpload(mockFile);

    expect(mockMutate).toHaveBeenCalledWith(
      { imageFile: mockFile, familyId: 'test-family-id', resize: true },
      expect.any(Object)
    );
    expect(uploadedImage.value).toBe(mockData.originalImageBase64);
    expect(detectedFaces.value).toEqual(mockData.detectedFaces);
    expect(originalImageUrl.value).toBe(mockData.originalImageUrl);
    expect(mockShowSnackbar).not.toHaveBeenCalled(); // No snackbar on success
  });

  it('handleFileUpload should show error snackbar on failed upload', async () => {
    const { selectedFamilyId, uploadedImage, detectedFaces, originalImageUrl, handleFileUpload } = mountComposable();
    selectedFamilyId.value = 'test-family-id';
    const mockFile = new File(['dummy content'], 'test.png', { type: 'image/png' });
    const mockErrorData = new Error('Upload failed');

    // Simulate mutation error
    mockMutate.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onError(mockErrorData);
    });

    await handleFileUpload(mockFile);

    expect(mockMutate).toHaveBeenCalledWith(
      { imageFile: mockFile, familyId: 'test-family-id', resize: true },
      expect.any(Object)
    );
    expect(mockShowSnackbar).toHaveBeenCalledWith(mockErrorData.message, 'error');
    // State should be reset by resetState() call at the beginning of handleFileUpload
    expect(uploadedImage.value).toBeNull();
    expect(detectedFaces.value).toEqual([]);
    expect(originalImageUrl.value).toBeNull();
  });

  it('should show error snackbar when detectError ref changes', async () => {
    mountComposable(); // Mount to set up watcher
    await nextTick(); // Ensure component is fully mounted and watchers are active

    const mockWatchError = new Error('Watch error');
    mockError.value = mockWatchError; // Trigger the watcher
    await nextTick(); // Wait for watcher to execute its effect

    expect(mockShowSnackbar).toHaveBeenCalledWith(mockWatchError.message, 'error');
  });
});
