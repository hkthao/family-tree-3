import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useFaceSearch } from '@/composables/face/logic/useFaceSearch';
import { ref } from 'vue';
import { type Composer } from 'vue-i18n';
import type { UseGlobalSnackbarReturn } from '@/composables/ui/useGlobalSnackbar';
import type { DetectedFace } from '@/types'; // Changed to type import
import type { UseDetectFacesMutationReturn } from '@/composables';

// Mock external dependencies
const mockShowSnackbar = vi.fn();
const mockUseGlobalSnackbar: () => UseGlobalSnackbarReturn = () => ({
  showSnackbar: mockShowSnackbar,
});

const mockUseI18n: () => Composer = () => ({
  t: vi.fn((key: string) => key),
}) as any;

const mockMutate = vi.fn();
const mockIsPending = ref<boolean>(false);
const mockDetectError = ref<Error | null>(null);

const mockUseDetectFacesMutation: () => UseDetectFacesMutationReturn = () => ({
  mutate: mockMutate,
  isPending: mockIsPending,
  error: mockDetectError,
}) as any; // Cast to any to bypass complex type issues

// Mock FileReader using vi.stubGlobal
const mockFileReaderInstance = {
  readAsDataURL: vi.fn(),
  onload: vi.fn(),
  result: null as string | ArrayBuffer | null,
};

vi.stubGlobal('FileReader', class {
  constructor() {
    return mockFileReaderInstance;
  }
});


vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn((options: any) => {
    const queryResult = {
      data: ref(options?.initialData || options?.placeholderData),
      isLoading: ref(false),
      isError: ref(false),
      error: ref<Error | null>(null),
      isFetching: ref(false),
      refetch: vi.fn(),
    };
    return queryResult;
  }),
  useMutation: vi.fn((options: any) => {
      const isPending = ref<boolean>(false);
      const error = ref<Error | null>(null);
      const mutate = vi.fn(async (variables, callbacks) => {
          isPending.value = true;
          try {
              const data = await options.mutationFn(variables);
              callbacks?.onSuccess?.(data, variables, null);
              return data;
          } catch (err) {
              error.value = err as Error;
              callbacks?.onError?.(err as Error, variables, null);
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
    { id: 'face1', memberId: 'member1', boundingBox: { x: 1, y: 1, width: 1, height: 1 }, thumbnail: 'base64', embedding: [1, 2, 3], status: "unrecognized" },
  ];
  const mockOriginalImageUrl = 'http://example.com/original.jpg';

  beforeEach(() => {
    vi.clearAllMocks();
    mockIsPending.value = false;
    mockDetectError.value = null;
    // Reset the mock FileReader instance before each test
    mockFileReaderInstance.readAsDataURL.mockClear();
    mockFileReaderInstance.onload = vi.fn();
    mockFileReaderInstance.result = null;
  });

  it('should initialize with correct default state', () => {
    const { state } = useFaceSearch({
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useDetectFacesMutation: mockUseDetectFacesMutation,
      FileReader: FileReader as any, // Cast to any because of stubGlobal
    });

    expect(state.selectedFamilyId.value).toBeUndefined();
    expect(state.uploadedImage.value).toBeUndefined();
    expect(state.detectedFaces.value).toEqual([]);
    expect(state.originalImageUrl.value).toBeNull();
    expect(state.isDetectingFaces.value).toBe(false);
  });

  it('should call showSnackbar on detectError change', async () => {
    useFaceSearch({
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useDetectFacesMutation: mockUseDetectFacesMutation,
      FileReader: FileReader as any, // Cast to any because of stubGlobal
    });

    mockDetectError.value = new Error('Detection failed');
    await vi.dynamicImportSettled();

    expect(mockShowSnackbar).toHaveBeenCalledWith('Detection failed', 'error');
  });

  it('should reset state correctly', () => {
    const { state, actions } = useFaceSearch({
      useI18n: mockUseI18n,
      useGlobalSnackbar: mockUseGlobalSnackbar,
      useDetectFacesMutation: mockUseDetectFacesMutation,
      FileReader: FileReader as any, // Cast to any because of stubGlobal
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
        FileReader: FileReader as any, // Cast to any because of stubGlobal
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
        FileReader: FileReader as any, // Cast to any because of stubGlobal
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
        FileReader: FileReader as any, // Cast to any because of stubGlobal
      });

      state.selectedFamilyId.value = 'family1';

      // Mock FileReader to simulate onload event
      mockFileReaderInstance.readAsDataURL.mockImplementation((_file: Blob) => {
        mockFileReaderInstance.result = 'base64image';
        // Simulate event dispatch
        (mockFileReaderInstance.onload as Mock)?.({ target: mockFileReaderInstance } as unknown as ProgressEvent<FileReader>);
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
        FileReader: FileReader as any, // Cast to any because of stubGlobal
      });

      state.selectedFamilyId.value = 'family1';

      mockFileReaderInstance.readAsDataURL.mockImplementation((_file: Blob) => {
        mockFileReaderInstance.result = 'base64image';
        // Simulate event dispatch
        (mockFileReaderInstance.onload as Mock)?.({ target: mockFileReaderInstance } as unknown as ProgressEvent<FileReader>);
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