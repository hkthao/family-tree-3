// src/composables/member-face/mutations/useAddMemberFaceMutation.ts
import { useMutation, type UseMutationOptions, type UseMutationReturnType, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { MemberFace, ApiError, AddMemberFaceDto } from '@/types';
import type { IMemberFaceService } from '@/services/member-face/member-face.service.interface';
import { queryKeys } from '@/constants/queryKeys';

interface UseAddMemberFaceMutationDeps {
  useMutation: <TData = unknown, TError = Error, TVariables = void, TContext = unknown>(
    options: UseMutationOptions<TData, TError, TVariables, TContext>,
  ) => UseMutationReturnType<TData, TError, TVariables, TContext>;
  getMemberFaceService: () => IMemberFaceService;
  useQueryClient: typeof useQueryClient;
}

const defaultDeps: UseAddMemberFaceMutationDeps = {
  useMutation,
  getMemberFaceService: () => useServices().memberFace,
  useQueryClient,
};

export function useAddMemberFaceMutation(
  deps: UseAddMemberFaceMutationDeps = defaultDeps,
) {
  const { useMutation: injectedUseMutation, getMemberFaceService, useQueryClient: injectedUseQueryClient } = deps;
  const memberFaceService = getMemberFaceService();
  const queryClient = injectedUseQueryClient();

  return injectedUseMutation<MemberFace, ApiError, AddMemberFaceDto>({
    mutationFn: async (memberFaceData: AddMemberFaceDto) => {
      const response = await memberFaceService.add(memberFaceData);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.memberFaces.all });
    },
  });
}

export type UseAddMemberFaceMutationReturn = ReturnType<typeof useAddMemberFaceMutation>;