import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useAddMemberFaceMutation } from '@/composables/member-face/mutations/useAddMemberFaceMutation';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import type { MemberFace } from '@/types';
import type { IMemberFaceService } from '@/services/member-face/member-face.service.interface';

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

// Mock memberFaceService
const mockMemberFaceService: IMemberFaceService = {
  detect: vi.fn(),
  add: vi.fn(),
  update: vi.fn(),
  delete: vi.fn(),
  getById: vi.fn(),
  search: vi.fn(),
  getByIds: vi.fn(),
};

// Mock queryClient
const mockQueryClient = {
  invalidateQueries: vi.fn(),
};

describe('useAddMemberFaceMutation', () => {
  const mockMemberFaceData: Omit<MemberFace, 'id'> = {
    memberId: 'member1',
    faceId: 'face1',
    boundingBox: { x: 10, y: 10, width: 20, height: 20 },
    confidence: 0.9,
    thumbnail: 'base64thumbnail',
    thumbnailUrl: 'http://example.com/thumbnail.jpg',
    originalImageUrl: 'http://example.com/original.jpg',
    embedding: [1, 2, 3],
    emotion: 'happy',
    emotionConfidence: 0.8,
    isVectorDbSynced: false,
    familyId: 'family1',
  };
  const mockMemberFace: MemberFace = { id: 'memberFace1', ...mockMemberFaceData };

  beforeEach(() => {
    vi.clearAllMocks();
    (useQueryClient as vi.Mock).mockReturnValue(mockQueryClient);
  });

  it('should call memberFaceService.add with correct data in mutationFn', async () => {
    // Mock useMutation to immediately execute mutationFn
    (useMutation as vi.Mock).mockImplementation((options) => {
      options.mutationFn(mockMemberFaceData);
      return {
        mutate: vi.fn(),
        isPending: false,
        error: null,
      };
    });
    mockMemberFaceService.add.mockResolvedValue({ ok: true, value: mockMemberFace });

    useAddMemberFaceMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockMemberFaceService, useQueryClient: useQueryClient as any });

    expect(mockMemberFaceService.add).toHaveBeenCalledWith(mockMemberFaceData);
  });

  it('should call onSuccess and invalidate queries on successful mutation', async () => {
    const onSuccessCallback = vi.fn();
    (useMutation as vi.Mock).mockImplementation((options) => {
      options.onSuccess();
      return {
        mutate: vi.fn((data, callbacks) => callbacks.onSuccess()),
        isPending: false,
        error: null,
      };
    });
    mockMemberFaceService.add.mockResolvedValue({ ok: true, value: mockMemberFace });

    const { mutate } = useAddMemberFaceMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockMemberFaceService, useQueryClient: useQueryClient as any });
    mutate(mockMemberFaceData, { onSuccess: onSuccessCallback });

    expect(onSuccessCallback).toHaveBeenCalled();
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.memberFaces.all });
  });

  it('should throw an error on failed mutation', async () => {
    const mockError = new Error('Failed to add member face');
    (useMutation as vi.Mock).mockImplementation((options) => {
      options.mutationFn = vi.fn(() => Promise.reject(mockError));
      return {
        mutate: vi.fn((data, callbacks) => {
          callbacks.onError(mockError);
          return Promise.reject(mockError);
        }),
        isPending: false,
        error: mockError,
      };
    });
    mockMemberFaceService.add.mockResolvedValue({ ok: false, error: mockError });

    const { mutate } = useAddMemberFaceMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockMemberFaceService, useQueryClient: useQueryClient as any });

    await expect(mutate(mockMemberFaceData, { onError: () => {} })).rejects.toThrow(mockError);
  });
});
