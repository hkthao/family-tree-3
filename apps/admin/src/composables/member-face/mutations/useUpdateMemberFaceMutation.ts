import { useMutation } from '@tanstack/vue-query';
import type { MemberFace, UpdateMemberFaceDto } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useUpdateMemberFaceMutation = () => {
  const services = useServices();
  return useMutation<MemberFace, Error, UpdateMemberFaceDto>({
    mutationFn: async (updateDto: UpdateMemberFaceDto) => {
      const payload: UpdateMemberFaceDto = { ...updateDto };
      if (typeof payload.embedding === 'string') {
        try {
          payload.embedding = JSON.parse(payload.embedding);
        } catch (e) {
          console.error("Failed to parse embedding string to number[]:", e);
          delete payload.embedding;
        }
      }

      const response = await services.memberFace.update(payload);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to update member face');
    },
  });
};
