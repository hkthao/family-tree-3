import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useDetectFacesMutation } from '@/composables/face/mutations/useDetectFacesMutation';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ref } from 'vue'; // Added this line
import { queryKeys } from '@/constants/queryKeys';
import type { DetectedFace } from '@/types';
import type { IMemberFaceService } from '@/services/member-face/member-face.service.interface';
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
    const data = ref(undefined);
    const error = ref<unknown | null>(null);
    const isIdle = ref(true);
    const isPending = ref(false);
    const isSuccess = ref(false);
    const isError = ref(false);
    const isPaused = ref(false); // Assuming not paused for this mock
    const status = ref('idle'); // 'idle' | 'pending' | 'success' | 'error'
    const variables = ref(undefined);
    const mutate = vi.fn(async (vars, callbacks) => {
      isIdle.value = false;
      isPending.value = true;
      isSuccess.value = false;
      isError.value = false;
      status.value = 'pending';
      error.value = null;
      data.value = undefined;
      variables.value = vars;
      try {
        const result = await options.mutationFn(vars);
        data.value = result;
        isPending.value = false;
        isSuccess.value = true;
        status.value = 'success';
        options.onSuccess?.(result, vars, null);
        callbacks?.onSuccess?.(result, vars, null);
        return result;
      } catch (err) {
        error.value = err;
        isPending.value = false;
        isError.value = true;
        status.value = 'error';
        options.onError?.(err, vars, null);
        callbacks?.onError?.(err, vars, null);
        throw err;
      }
    });
    const reset = vi.fn(() => {
      data.value = undefined;
      error.value = null;
      isIdle.value = true;
      isPending.value = false;
      isSuccess.value = false;
      isError.value = false;
      status.value = 'idle';
      variables.value = undefined;
    });
    return {
      mutate,
      mutateAsync: mutate, // often mutateAsync is just mutate in mocks
      data,
      error,
      isIdle,
      isPending,
      isLoading: isPending, // isLoading is an alias for isPending
      isSuccess,
      isError,
      isPaused,
      status,
      variables,
      reset,
    };
  }),
  useQueryClient: vi.fn(() => ({
    invalidateQueries: vi.fn(),
    setQueryData: vi.fn(),
    getQueryData: vi.fn(),
  })),
}));

// Mock memberFaceService
const mockMemberFaceService: IMemberFaceService = {
  detect: vi.fn() as Mock,
  add: vi.fn() as Mock,
  getById: vi.fn() as Mock,
  update: vi.fn() as Mock,
  search: vi.fn() as Mock,
  delete: vi.fn() as Mock,
  getByIds: vi.fn() as Mock,
};

// Mock queryClient
const mockQueryClient = {
  invalidateQueries: vi.fn(),
};

describe('useDetectFacesMutation', () => {
  const mockImageFile = new File(['dummy content'], 'test.jpg', { type: 'image/jpeg' });
  const mockDetectedFaces: DetectedFace[] = [
    {
      id: 'face1', memberId: 'member1', boundingBox: { x: 1, y: 1, width: 1, height: 1 }, thumbnail: 'base64',
      embedding: null,
      status: 'unrecognized'
    },
  ];
  const mockOriginalImageUrl = 'http://example.com/original.jpg';

  beforeEach(() => {
    vi.clearAllMocks();
    (useQueryClient as Mock).mockReturnValue(mockQueryClient);
  });

  it('should call faceService.detect with correct data in mutationFn', async () => {
    (mockMemberFaceService.detect as Mock).mockResolvedValue({ ok: true, value: { detectedFaces: mockDetectedFaces, originalImageUrl: mockOriginalImageUrl } });

    const { mutate } = useDetectFacesMutation({ useMutation, getMemberFaceService: () => mockMemberFaceService, useQueryClient });
    await mutate({ imageFile: mockImageFile, familyId: 'family1', resize: true });

    expect(mockMemberFaceService.detect).toHaveBeenCalledWith(mockImageFile, 'family1', true);
  });

  it('should call onSuccess and invalidate queries on successful mutation', async () => {
    const onSuccessCallback = vi.fn();
    (mockMemberFaceService.detect as Mock).mockResolvedValue({ ok: true, value: { detectedFaces: mockDetectedFaces, originalImageUrl: mockOriginalImageUrl } });

    const { mutate } = useDetectFacesMutation({ useMutation, getMemberFaceService: () => mockMemberFaceService, useQueryClient });
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
    (useMutation as Mock).mockImplementation((_options) => {
      return {
        mutate: vi.fn((data, callbacks) => {
          callbacks.onError(mockError);
          return Promise.reject(mockError);
        }),
        isPending: false,
        error: mockError,
      };
    });
    (mockMemberFaceService.detect as Mock).mockResolvedValue({ ok: false, error: mockError });

    const { mutate } = useDetectFacesMutation({ useMutation, getMemberFaceService: () => mockMemberFaceService, useQueryClient });

    await expect(mutate({ imageFile: mockImageFile, familyId: 'family1', resize: true }, { onError: () => {} })).rejects.toThrow(mockError);
  });
});
