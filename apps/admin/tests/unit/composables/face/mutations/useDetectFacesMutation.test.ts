import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useDetectFacesMutation } from '@/composables/face/mutations/useDetectFacesMutation';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ref } from 'vue'; // Added this line
import { queryKeys } from '@/constants/queryKeys';
import type { DetectedFace } from '@/types';
import type { IFaceService } from '@/services/face/face.service.interface';

// Mock the external dependencies
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn((options) => {
    return {
      data: ref(options?.initialData || options?.placeholderData),
      isLoading: ref(false),
      isError: ref(false),
      error: ref(null),
      isFetching: ref(false),
      refetch: vi.fn(),
    };
  }),
  useMutation: vi.fn((options) => {
    const isPending = ref(false);
    const error = ref(null);
    const mutate = vi.fn(async (variables, callbacks) => {
      isPending.value = true;
      try {
        const data = await options.mutationFn(variables);
        isPending.value = false;
        options.onSuccess?.(data, variables, null); // Call options.onSuccess
        callbacks?.onSuccess?.(data, variables, null); // Also call callbacks.onSuccess
        return data;
      } catch (err) {
        error.value = err;
        isPending.value = false;
        options.onError?.(err, variables, null); // Call options.onError
        callbacks?.onError?.(err, variables, null); // Also call callbacks.onError
        throw err;
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

// Mock faceService
const mockFaceService: IFaceService = {
  detect: vi.fn(),
  getById: vi.fn(),
  search: vi.fn(),
  update: vi.fn(),
  delete: vi.fn(),
};

// Mock queryClient
const mockQueryClient = {
  invalidateQueries: vi.fn(),
};

describe('useDetectFacesMutation', () => {
  const mockImageFile = new File(['dummy content'], 'test.jpg', { type: 'image/jpeg' });
  const mockDetectedFaces: DetectedFace[] = [
    { id: 'face1', memberId: 'member1', boundingBox: { x: 1, y: 1, width: 1, height: 1 }, thumbnail: 'base64' },
  ];
  const mockOriginalImageUrl = 'http://example.com/original.jpg';

  beforeEach(() => {
    vi.clearAllMocks();
    (useQueryClient as vi.Mock).mockReturnValue(mockQueryClient);
  });

  it('should call faceService.detect with correct data in mutationFn', async () => {
    mockFaceService.detect.mockResolvedValue({ ok: true, value: { detectedFaces: mockDetectedFaces, originalImageUrl: mockOriginalImageUrl } });

    const { mutate } = useDetectFacesMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockFaceService, useQueryClient: useQueryClient as any });
    await mutate({ imageFile: mockImageFile, familyId: 'family1', resize: true });

    expect(mockFaceService.detect).toHaveBeenCalledWith(mockImageFile, 'family1', true);
  });

  it('should call onSuccess and invalidate queries on successful mutation', async () => {
    const onSuccessCallback = vi.fn();
    mockFaceService.detect.mockResolvedValue({ ok: true, value: { detectedFaces: mockDetectedFaces, originalImageUrl: mockOriginalImageUrl } });

    const { mutate } = useDetectFacesMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockFaceService, useQueryClient: useQueryClient as any });
    await mutate({ imageFile: mockImageFile, familyId: 'family1', resize: true }, { onSuccess: onSuccessCallback });

    expect(onSuccessCallback).toHaveBeenCalledWith(
      { detectedFaces: mockDetectedFaces, originalImageUrl: mockOriginalImageUrl },
      { imageFile: mockImageFile, familyId: 'family1', resize: true },
      null,
    );
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.memberFaces.all });
  });

  it('should throw an error on failed mutation', async () => {
    const mockError = new Error('Failed to detect faces');
    (useMutation as vi.Mock).mockImplementation((_options) => {
      return {
        mutate: vi.fn((data, callbacks) => {
          callbacks.onError(mockError);
          return Promise.reject(mockError);
        }),
        isPending: false,
        error: mockError,
      };
    });
    mockFaceService.detect.mockResolvedValue({ ok: false, error: mockError });

    const { mutate } = useDetectFacesMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockFaceService, useQueryClient: useQueryClient as any });

    await expect(mutate({ imageFile: mockImageFile, familyId: 'family1', resize: true }, { onError: () => {} })).rejects.toThrow(mockError);
  });
});
