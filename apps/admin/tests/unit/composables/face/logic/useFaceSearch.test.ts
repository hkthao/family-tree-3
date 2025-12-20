import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useFaceSearch } from '@/composables/face/logic/useFaceSearch';
import { ref } from 'vue';
import type { Composer } from 'vue-i18n';
import type { UseGlobalSnackbarReturn } from '@/composables/ui/useGlobalSnackbar';
import type { UseDetectFacesMutationReturn } from '@/composables/face/mutations/useDetectFacesMutation';
import type { DetectedFace } from '@/types';

// Mock external dependencies
const mockShowSnackbar = vi.fn();
const mockUseGlobalSnackbar: () => UseGlobalSnackbarReturn = () => ({
  showSnackbar: mockShowSnackbar,
});

const mockUseI18n: () => Composer = () => ({
  t: vi.fn((key: string) => key),
}) as Composer;

const mockMutate = vi.fn();
const mockIsPending = ref(false);
const mockDetectError = ref(null);

const mockUseDetectFacesMutation: () => UseDetectFacesMutationReturn = () => ({
  mutate: mockMutate,
  isPending: mockIsPending,
  error: mockDetectError,
});

const mockFileReader = vi.fn(() => ({
  readAsDataURL: vi.fn(),
  onload: vi.fn(),
}));

vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn((options) => {
    const queryResult = {
      data: ref(options?.initialData || options?.placeholderData),
      isLoading: ref(false),
      isError: ref(false),
      error: ref(null),
      isFetching: ref(false),
      refetch: vi.fn(),
    };
    return queryResult;
  }),
  useMutation: vi.fn((options) => {
      const isPending = ref(false);
      const error = ref(null);
      const mutate = vi.fn(async (variables, callbacks) => {
          isPending.value = true;
          try {
              const data = await options.mutationFn(variables);
              callbacks?.onSuccess?.(data, variables, null);
              return data;
          } catch (err) {
              error.value = err;
              callbacks?.onError?.(err, variables, null);
              throw err;
          } finally {
              isPending.value = false;
          }
      });
      return {
          mutate,
          isPending,
          error,
      };
  }),
  useQueryClient: vi.fn(() => ({
    invalidateQueries: vi.fn(),
    setQueryData: vi.fn(),
    getQueryData: vi.fn(),
  })),
}));

describe('useFaceSearch', () => {
  const mockFile = new File(['dummy content'], 'test.jpg', { type: 'image/jpeg' });
  const mockDetectedFaces: DetectedFace[] = [
    { id: 'face1', memberId: 'member1', boundingBox: { x: 1, y: 1, width: 1, height: 1 }, thumbnail: 'base64' },
  ];
  const mockOriginalImageUrl = 'http://example.com/original.jpg';

  beforeEach(() => {
    vi.clearAllMocks();
    mockIsPending.value = false;
    mockDetectError.value = null;
  });

  it('should initialize with correct default state', () => {
    const { state } = useFaceSearch({
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useDetectFacesMutation: mockUseDetectFacesMutation,
      FileReader: mockFileReader,
    });

    expect(state.selectedFamilyId.value).toBeUndefined();
    expect(state.uploadedImage.value).toBeUndefined();
    expect(state.detectedFaces.value).toEqual([]);
    expect(state.originalImageUrl.value).toBeNull(); // Changed to toBeNull()
    expect(state.isDetectingFaces.value).toBe(false);
  });

  it('should call showSnackbar on detectError change', async () => {
    useFaceSearch({
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useDetectFacesMutation: mockUseDetectFacesMutation,
      FileReader: mockFileReader,
    });

    mockDetectError.value = new Error('Detection failed');
    await vi.dynamicImportSettled(); // Wait for watchers to trigger

    expect(mockShowSnackbar).toHaveBeenCalledWith('Detection failed', 'error');
  });

  it('should reset state correctly', () => {
    const { state, actions } = useFaceSearch({
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useDetectFacesMutation: mockUseDetectFacesMutation,
      FileReader: mockFileReader,
    });

    state.uploadedImage.value = 'some-image';
    state.detectedFaces.value = mockDetectedFaces;
    state.originalImageUrl.value = 'some-url';

    actions.resetState();

    expect(state.uploadedImage.value).toBeNull();
    expect(state.detectedFaces.value).toEqual([]);
    expect(state.originalImageUrl.value).toBeNull();
  });

  describe('handleFileUpload', () => {
    it('should reset state and return if no file is provided', async () => {
      const { state, actions } = useFaceSearch({
        useI18n: mockUseI18n,
        useGlobalSnackbar: mockUseGlobalSnackbar,
        useDetectFacesMutation: mockUseDetectFacesMutation,
        FileReader: mockFileReader,
      });

      state.uploadedImage.value = 'some-image';
      await actions.handleFileUpload(null);

      expect(state.uploadedImage.value).toBeNull();
      expect(mockShowSnackbar).not.toHaveBeenCalled();
      expect(mockMutate).not.toHaveBeenCalled();
    });

    it('should show snackbar warning if no familyId is selected', async () => {
      const { state, actions } = useFaceSearch({
        useI18n: mockUseI18n,
        useGlobalSnackbar: mockUseGlobalSnackbar,
        useDetectFacesMutation: mockUseDetectFacesMutation,
        FileReader: mockFileReader,
      });

      state.selectedFamilyId.value = undefined;
      await actions.handleFileUpload(mockFile);

      expect(mockShowSnackbar).toHaveBeenCalledWith('face.selectFamilyToUpload', 'warning');
      expect(mockMutate).not.toHaveBeenCalled();
    });

    it('should call detectFaces mutation on successful file upload with selectedFamilyId', async () => {
      const { state, actions } = useFaceSearch({
        useI18n: mockUseI18n,
        useGlobalSnackbar: mockUseGlobalSnackbar,
        useDetectFacesMutation: mockUseDetectFacesMutation,
        FileReader: mockFileReader,
      });

      state.selectedFamilyId.value = 'family1';

      // Mock FileReader to simulate onload event
      mockFileReader.mockImplementation(() => {
        const reader = new EventTarget() as FileReader;
        reader.readAsDataURL = vi.fn(() => {
          reader.onload?.({ target: { result: 'base64image' } } as ProgressEvent<FileReader>);
        });
        reader.onload = null; // Reset onload for subsequent calls
        return reader;
      });

      mockMutate.mockImplementation((_variables, callbacks) => {
        callbacks.onSuccess({ detectedFaces: mockDetectedFaces, originalImageUrl: mockOriginalImageUrl });
      });

      await actions.handleFileUpload(mockFile);

      expect(mockMutate).toHaveBeenCalledWith(
        { imageFile: mockFile, familyId: 'family1', resize: true },
        expect.any(Object), // Expecting callbacks object
      );
      expect(state.uploadedImage.value).toBe('base64image');
      expect(state.detectedFaces.value).toEqual(mockDetectedFaces);
      expect(state.originalImageUrl.value).toBe(mockOriginalImageUrl);
      expect(mockShowSnackbar).not.toHaveBeenCalled();
    });

    it('should show snackbar error and clear state on detectFaces mutation error', async () => {
      const { state, actions } = useFaceSearch({
        useI18n: mockUseI18n,
        useGlobalSnackbar: mockUseGlobalSnackbar,
        useDetectFacesMutation: mockUseDetectFacesMutation,
        FileReader: mockFileReader,
      });

      state.selectedFamilyId.value = 'family1';

      mockFileReader.mockImplementation(() => {
        const reader = new EventTarget() as FileReader;
        reader.readAsDataURL = vi.fn(() => {
          reader.onload?.({ target: { result: 'base64image' } } as ProgressEvent<FileReader>);
        });
        reader.onload = null;
        return reader;
      });

      const mockError = new Error('API error');
      mockMutate.mockImplementation((_variables, callbacks) => {
        callbacks.onError(mockError);
      });

      await actions.handleFileUpload(mockFile);

      expect(mockShowSnackbar).toHaveBeenCalledWith('API error', 'error');
      expect(state.uploadedImage.value).toBeNull();
      expect(state.detectedFaces.value).toEqual([]);
    });
  });
});