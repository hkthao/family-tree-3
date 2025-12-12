import { useMutation } from '@tanstack/vue-query';
import type { DetectedFace, FaceDetectionRessult } from '@/types';
import { useServices } from '@/plugins/services.plugin';

interface DetectFacesPayload {
  imageFile: File;
  familyId: string;
  resize: boolean;
}

export const useDetectFacesMutation = () => {
  const services = useServices();
  return useMutation({
    mutationFn: async ({ imageFile, familyId, resize }: DetectFacesPayload) => {
      const response = await services.memberFace.detect(imageFile, familyId, resize);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to detect faces');
    },
  });
};
