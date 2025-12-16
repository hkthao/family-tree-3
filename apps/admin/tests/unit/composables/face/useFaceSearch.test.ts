import { describe, it, expect, vi, beforeEach } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import { ref, type Ref, nextTick } from 'vue';
import { useFaceSearch } from '@/composables/face/useFaceSearch';
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
const mockError: Ref<Error | null> = ref(null);
vi.mock('@/composables/member-face', () => ({
  useDetectFacesMutation: vi.fn(() => ({
    mutate: mockMutate,
    isPending: mockIsPending,
    error: mockError,
  })),
}));

// Mock FileReader
const mockFileReader = {
  result: 'data:image/png;base64,mock-local-base64',
  onload: null as any,
  readAsDataURL: vi.fn(function(this: any, _file: File) {
    this.onload({ target: { result: this.result } });
  }),
  // Mock event target for onload
};

vi.spyOn(window, 'FileReader').mockImplementation(() => mockFileReader as any);

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
    mockFileReader.readAsDataURL.mockClear();
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
    detectedFaces.value = [{ id: '1', boundingBox: { x: 0, y: 0, width: 10, height: 10 } }] as DetectedFace[];
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
    expect(mockFileReader.readAsDataURL).not.toHaveBeenCalled();
  });

  it('handleFileUpload should show warning if no familyId is selected', async () => {
    const { handleFileUpload } = mountComposable();
    const mockFile = new File(['dummy content'], 'test.png', { type: 'image/png' });

    await handleFileUpload(mockFile);

    expect(mockShowSnackbar).toHaveBeenCalledWith('face.selectFamilyToUpload', 'warning');
    expect(mockMutate).not.toHaveBeenCalled();
    expect(mockFileReader.readAsDataURL).not.toHaveBeenCalled();
  });

  it('handleFileUpload should call detectFaces on successful upload and display local image', async () => {
    const { selectedFamilyId, uploadedImage, detectedFaces, originalImageUrl, handleFileUpload } = mountComposable();
    selectedFamilyId.value = 'test-family-id';
    const mockFile = new File(['dummy content'], 'test.png', { type: 'image/png' });

    const mockDetectedFaces = [{ id: 'face1', boundingBox: { x: 0, y: 0, width: 10, height: 10 } }] as DetectedFace[];
    const mockOriginalImageUrl = 'original-url-mock';

    // Simulate mutation success
    mockMutate.mockImplementationOnce((_payload, callbacks) => {
      callbacks.onSuccess({ detectedFaces: mockDetectedFaces, originalImageUrl: mockOriginalImageUrl });
    });

    await handleFileUpload(mockFile);

    expect(mockFileReader.readAsDataURL).toHaveBeenCalledWith(mockFile);
    expect(uploadedImage.value).toBe(mockFileReader.result); // Should be the local Data URL

    expect(mockMutate).toHaveBeenCalledWith(
      { imageFile: mockFile, familyId: 'test-family-id', resize: true },
      expect.any(Object)
    );
    expect(detectedFaces.value).toEqual(mockDetectedFaces);
    expect(originalImageUrl.value).toBe(mockOriginalImageUrl);
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

    expect(mockFileReader.readAsDataURL).toHaveBeenCalledWith(mockFile);

    expect(mockMutate).toHaveBeenCalledWith(
      { imageFile: mockFile, familyId: 'test-family-id', resize: true },
      expect.any(Object)
    );
    expect(mockShowSnackbar).toHaveBeenCalledWith(mockErrorData.message, 'error');
    // On error, uploadedImage and detectedFaces should be cleared
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
