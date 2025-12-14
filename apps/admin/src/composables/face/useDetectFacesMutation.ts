import { useMutation } from '@tanstack/vue-query';
import { useServices } from '@/composables/utils/useServices';
import type { FaceDetectionRessult, ApiError } from '@/types';
import type { IMemberFaceService } from '@/services/member-face/member-face.service.interface';

interface DetectFacesPayload {
  imageFile: File;
  familyId: string;
  resizeImageForAnalysis: boolean;
}

export function useDetectFacesMutation() {
  const { memberFace: memberFaceService } = useServices();

  return useMutation<FaceDetectionRessult, ApiError, DetectFacesPayload>({
    mutationFn: async ({ imageFile, familyId, resizeImageForAnalysis }) => {
      const result = await (memberFaceService as IMemberFaceService).detect(imageFile, familyId, resizeImageForAnalysis);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
  });
}
