import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useAddMemberFaceMutation } from '@/composables/member-face/mutations/useAddMemberFaceMutation';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ref } from 'vue';
import { queryKeys } from '@/constants/queryKeys';
import type { MemberFace, CreateMemberFaceCommand } from '@/types';
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
  }) as Mock,
  useMutation: vi.fn((options: any) => {
      const isPending = ref(false);
      const error = ref<unknown | null>(null);
      const mutate = vi.fn(async (variables, callbacks) => {
          isPending.value = true;
          try {
              const data = await options.mutationFn(variables);
              options?.onSuccess?.(data, variables, null); // Call options.onSuccess
              callbacks?.onSuccess?.(data, variables, null);
              return data;
          } catch (err) {
              error.value = err;
              options?.onError?.(err, variables, null); // Call options.onError
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
  }) as Mock,
  useQueryClient: vi.fn(() => ({
    invalidateQueries: vi.fn(),
    setQueryData: vi.fn(),
    getQueryData: vi.fn(),
  })) as Mock,
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
  exportMemberFaces: vi.fn(),
  importMemberFaces: vi.fn(),
  getMemberFacesByMemberId: vi.fn(), // Add this
};

// Mock queryClient
const mockQueryClient = {
  invalidateQueries: vi.fn(),
};

describe('useAddMemberFaceMutation', () => {
  const mockMemberFaceData: CreateMemberFaceCommand = {
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
    familyId: 'family1' as string,
  };
  const mockMemberFace: MemberFace = { id: 'memberFace1', ...mockMemberFaceData, isVectorDbSynced: mockMemberFaceData.isVectorDbSynced || false };

  beforeEach(() => {
    vi.clearAllMocks();
    (useQueryClient as Mock).mockReturnValue(mockQueryClient);
  });

  it('should call memberFaceService.add with correct data in mutationFn', async () => {
    // Mock useMutation to immediately execute mutationFn
    (useMutation as Mock).mockImplementation((options: any) => {
      options.mutationFn(mockMemberFaceData);
      return {
        mutate: vi.fn(),
        isPending: false,
        error: null,
      };
    });
    (mockMemberFaceService.add as Mock).mockResolvedValue({ ok: true, value: mockMemberFace });

    useAddMemberFaceMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockMemberFaceService, useQueryClient: useQueryClient as any });

    expect(mockMemberFaceService.add).toHaveBeenCalledWith(mockMemberFaceData);
  });

  it('should call onSuccess and invalidate queries on successful mutation', async () => {
    const onSuccessCallback = vi.fn();
    (useMutation as Mock).mockImplementation((options: any) => {
      options.onSuccess();
      return {
        mutate: vi.fn((data, callbacks) => callbacks.onSuccess()),
        isPending: false,
        error: null,
      };
    });
    (mockMemberFaceService.add as Mock).mockResolvedValue({ ok: true, value: mockMemberFace });

    const { mutate } = useAddMemberFaceMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockMemberFaceService, useQueryClient: useQueryClient as any });
    mutate(mockMemberFaceData, { onSuccess: onSuccessCallback });

    expect(onSuccessCallback).toHaveBeenCalled();
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.memberFaces.all });
  });

  it('should throw an error on failed mutation', async () => {
    const mockError = new Error('Failed to add member face');
    (useMutation as Mock).mockImplementation((options: any) => {
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
    (mockMemberFaceService.add as Mock).mockResolvedValue({ ok: false, error: mockError });

    const { mutate } = useAddMemberFaceMutation({ useMutation: useMutation as any, getMemberFaceService: () => mockMemberFaceService, useQueryClient: useQueryClient as any });

    await expect(mutate(mockMemberFaceData, { onError: () => {} })).rejects.toThrow(mockError);
  });
});
