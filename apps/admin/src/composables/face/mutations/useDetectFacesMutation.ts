// src/composables/face/mutations/useDetectFacesMutation.ts
import { useMutation, type UseMutationOptions, type UseMutationReturnType, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables';
import type { FaceDetectionRessult, ApiError } from '@/types';
import type { IMemberFaceService } from '@/services/member-face/member-face.service.interface';
import { queryKeys } from '@/constants/queryKeys';

interface DetectFacesCommand {
  imageFile: File;
  familyId: string;
  resize: boolean;
}

interface UseDetectFacesMutationDeps {
  useMutation: <TData = unknown, TError = Error, TVariables = void, TContext = unknown>(
    options: UseMutationOptions<TData, TError, TVariables, TContext>,
  ) => UseMutationReturnType<TData, TError, TVariables, TContext>;
  getMemberFaceService: () => IMemberFaceService;
  useQueryClient: typeof useQueryClient;
}

const defaultDeps: UseDetectFacesMutationDeps = {
  useMutation,
  getMemberFaceService: () => useServices().memberFace,
  useQueryClient,
};

export function useDetectFacesMutation(
  deps: UseDetectFacesMutationDeps = defaultDeps,
) {
  const { useMutation: injectedUseMutation, getMemberFaceService, useQueryClient: injectedUseQueryClient } = deps;
  const memberFaceService = getMemberFaceService();
  const queryClient = injectedUseQueryClient();

  return injectedUseMutation<FaceDetectionRessult, ApiError, DetectFacesCommand>({
    mutationFn: async (command: DetectFacesCommand) => {
      const response = await memberFaceService.detect(command.imageFile, command.familyId, command.resize);
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

export type UseDetectFacesMutationReturn = ReturnType<typeof useDetectFacesMutation>;
