import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useFaceMemberSelectDialog } from '@/composables/face/logic/useFaceMemberSelectDialog';
import { useQuery } from '@tanstack/vue-query';
import { ref, nextTick, reactive, type Ref } from 'vue';
import { type DetectedFace, type MemberDto, type ApiError, Gender } from '@/types';
import type { IMemberService } from '@/services/member/member.service.interface';

import type { Composer } from 'vue-i18n';

// Mock the external dependencies
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
      const isPending = ref(false);
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

// Mock useI18n
const mockUseI18n = vi.fn(() => ({
  t: vi.fn((key: string) => key),
})) as unknown as () => Composer;

// Mock memberService
const mockMemberService: IMemberService = {
  fetchMembersByFamilyId: vi.fn() as Mock,
  add: vi.fn() as Mock,
  update: vi.fn() as Mock,
  delete: vi.fn() as Mock,
  getById: vi.fn() as Mock,
  search: vi.fn() as Mock,
  addItems: vi.fn() as Mock,
  updateMemberBiography: vi.fn() as Mock,
  getRelatives: vi.fn() as Mock,
  getByIds: vi.fn() as Mock,
  exportMembers: vi.fn() as Mock,
  importMembers: vi.fn() as Mock,
};

describe('useFaceMemberSelectDialog', () => {
  const mockDetectedFace: DetectedFace = {
    id: 'face1',
    memberId: 'member1',
    memberName: 'John Doe',
    relationPrompt: 'Father',
    thumbnail: 'base64thumbnail',
    boundingBox: { x: 10, y: 10, width: 20, height: 20 },
    embedding: [1, 2, 3], // Changed to number[]
    status: "unrecognized", // Changed to FaceStatus enum
  };

  const mockMember: MemberDto = {
    id: 'member1',
    fullName: 'John Doe',
    gender: Gender.Male, // Changed to Gender enum
    familyId: 'family1',
    lastName: '',
    firstName: ''
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

  it('should call memberService.getById in queryFn when selectedMemberId is defined', async () => {
    let capturedQueryFn: (() => Promise<MemberDto | undefined>) | undefined;

    (useQuery as Mock).mockImplementation((options: any) => {
      capturedQueryFn = options.queryFn;
      return {
        data: ref(mockMember),
        isLoading: ref(false),
        isError: ref(false),
        error: ref<Error | null>(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });
    (mockMemberService.getById as Mock).mockResolvedValue({ ok: true, value: mockMember });

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
    (useQuery as Mock).mockImplementation(() => {
      return {
        data: ref(mockMember),
        isLoading: ref(false),
        isError: ref(false),
        error: ref<Error | null>(null),
        isFetching: ref(false),
        refetch: vi.fn(),
      };
    });
    (mockMemberService.getById as Mock).mockResolvedValue({ ok: true, value: mockMember });

    const { state } = useFaceMemberSelectDialog(mockProps, mockEmit as any, {
      useI18n: mockUseI18n,
      useQuery: useQuery as any,
      getMemberService: () => mockMemberService,
    });

    expect(state.selectedMemberDetails.value).toEqual(mockMember);
  });

  it('should handle query error and set selectedMemberDetails to undefined', async () => {
    const mockError: ApiError = { message: 'MemberDto not found', statusCode: 404, name: 'ApiError' };
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError));
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(true),
        error: ref<Error | null>(mockError),
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
    const member = { id: 'member1', fullName: 'John Doe', gender: Gender.Male, familyId: 'family1' }; // Changed to Gender enum
    const face = { ...mockDetectedFace, memberId: 'member1' };
    const updatedFace: DetectedFace = { ...face, memberName: member.fullName, relationPrompt: mockDetectedFace.relationPrompt };

    (useQuery as Mock).mockImplementation(() => {
      return {
        data: ref(member), // Simulate selectedMemberDetails being available
        isLoading: ref(false),
        isError: ref(false),
        error: ref<Error | null>(null),
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

    (useQuery as Mock).mockImplementation(() => {
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(false),
        error: ref<Error | null>(null),
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

    (useQuery as Mock).mockImplementation((options: any) => {
      enabledComputed = options.enabled;
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(false),
        error: ref<Error | null>(null),
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

    (useQuery as Mock).mockImplementation((options: any) => {
      enabledComputed = options.enabled;
      return {
        data: ref(undefined),
        isLoading: ref(false),
        isError: ref(false),
        error: ref<Error | null>(null),
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

