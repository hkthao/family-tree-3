import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useFaceMemberSelectDialog } from '@/composables/face/logic/useFaceMemberSelectDialog';
import { useQuery } from '@tanstack/vue-query';
import { ref, nextTick, reactive } from 'vue';
import type { DetectedFace, Member, ApiError } from '@/types';
import type { IMemberService } from '@/services/member/member.service.interface';

import type { Composer } from 'vue-i18n';

// Mock the external dependencies
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

// Mock useI18n
const mockUseI18n = vi.fn(() => ({
  t: vi.fn((key: string) => key),
})) as unknown as () => Composer;

// Mock memberService
const mockMemberService: IMemberService = {
  fetchMembersByFamilyId: vi.fn(),
  add: vi.fn(),
  update: vi.fn(),
  delete: vi.fn(),
  getById: vi.fn(),
  search: vi.fn(),
  addItems: vi.fn(),
  updateMemberBiography: vi.fn(),
  getRelatives: vi.fn(),
};

describe('useFaceMemberSelectDialog', () => {
  const mockDetectedFace: DetectedFace = {
    id: 'face1',
    memberId: 'member1',
    memberName: 'John Doe',
    relationPrompt: 'Father',
    thumbnail: 'base64thumbnail',
    boundingBox: { x: 10, y: 10, width: 20, height: 20 },
  };

  const mockMember: Member = {
    id: 'member1',
    fullName: 'John Doe',
    gender: 0,
    familyId: 'family1',
  };

  const mockProps = {
    show: true,
    selectedFace: mockDetectedFace,
    familyId: 'family1',
    showRelationPromptField: true,
    disableSaveValidation: false,
  };

  const mockEmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should initialize with correct initial state', () => {
    const { state } = useFaceMemberSelectDialog(mockProps, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });

    expect(state.selectedMemberId.value).toBe(mockDetectedFace.memberId);
    expect(state.internalRelationPrompt.value).toBe(mockDetectedFace.relationPrompt);
    expect(state.faceThumbnailSrc.value).toBe(`data:image/jpeg;base64,${mockDetectedFace.thumbnail}`);
  });

  it('should update selectedMemberId and internalRelationPrompt when selectedFace changes', async () => {
    const newFace: DetectedFace = {
      ...mockDetectedFace,
      memberId: 'member2',
      relationPrompt: 'Mother',
    };
    const props = reactive({ ...mockProps, selectedFace: null as any }); // Start with null
    
    // Create composable
    const { state } = useFaceMemberSelectDialog(props, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });

    // Update prop
    props.selectedFace = newFace;

    // Await next tick to allow watcher to run
    await nextTick(); 

    expect(state.selectedMemberId.value).toBe('member2');
    expect(state.internalRelationPrompt.value).toBe('Mother');
  });

       it('should call memberService.getById in queryFn when selectedMemberId is defined', async () => {    let capturedQueryFn: (() => Promise<Member | undefined>) | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      capturedQueryFn = options.queryFn;
      return {
        data: ref(mockMember),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });
    mockMemberService.getById.mockResolvedValue({ ok: true, value: mockMember });

    const { state } = useFaceMemberSelectDialog(mockProps, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });
    
    // Manually set selectedMemberId to trigger queryFn
    state.selectedMemberId.value = 'member1'; 

    // Now, manually execute the captured queryFn to trigger the service call
    await capturedQueryFn?.();

    expect(mockMemberService.getById).toHaveBeenCalledWith('member1');
  });

  it('should return member details on successful query', async () => {
    (useQuery as vi.Mock).mockImplementation(() => {
      return {
        data: ref(mockMember),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });
    mockMemberService.getById.mockResolvedValue({ ok: true, value: mockMember });

    const { state } = useFaceMemberSelectDialog(mockProps, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });

    expect(state.selectedMemberDetails.value).toEqual(mockMember);
  });

  it('should handle query error and set selectedMemberDetails to undefined', async () => {
    const mockError: ApiError = { message: 'Member not found', statusCode: 404 };
    (useQuery as vi.Mock).mockImplementation((options) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError));
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(true),
        error: ref(mockError),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    const { state } = useFaceMemberSelectDialog(mockProps, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });

    expect(state.selectedMemberDetails.value).toBeUndefined();
  });

  it('should emit "label-face" and "update:show" on handleSave with valid data', () => {
    const member = { id: 'member1', fullName: 'John Doe', gender: 0, familyId: 'family1' };
    const face = { ...mockDetectedFace, memberId: 'member1' };
    const updatedFace: DetectedFace = { ...face, memberName: member.fullName, relationPrompt: mockDetectedFace.relationPrompt };

    (useQuery as vi.Mock).mockImplementation(() => {
      return {
        data: ref(member), // Simulate selectedMemberDetails being available
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    const { actions } = useFaceMemberSelectDialog({ ...mockProps, selectedFace: face }, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });
    actions.handleSave();

    expect(mockEmit).toHaveBeenCalledWith('label-face', updatedFace);
    expect(mockEmit).toHaveBeenCalledWith('update:show', false);
  });

  it('should not emit on handleSave if selectedFace or selectedMemberId is missing', () => {
    const face = { ...mockDetectedFace, memberId: undefined as any };

    (useQuery as vi.Mock).mockImplementation(() => {
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    const { actions } = useFaceMemberSelectDialog({ ...mockProps, selectedFace: face }, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });
    actions.handleSave();

    expect(mockEmit).not.toHaveBeenCalledWith('label-face', expect.anything());
    expect(mockEmit).not.toHaveBeenCalledWith('update:show', expect.anything());
  });



  it('should set enabled to true when selectedMemberId is defined', () => {
    let enabledComputed: Ref<boolean> | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      enabledComputed = options.enabled;
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    const { state } = useFaceMemberSelectDialog(mockProps, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });

    state.selectedMemberId.value = 'someId';
    expect(enabledComputed?.value).toBe(true);
  });



  it('should set enabled to false when selectedMemberId is undefined', () => {
    let enabledComputed: Ref<boolean> | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      enabledComputed = options.enabled;
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(false),
        error: ref(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });

    const { state } = useFaceMemberSelectDialog(mockProps, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });

    state.selectedMemberId.value = undefined;
    expect(enabledComputed?.value).toBe(false);
  });
});
