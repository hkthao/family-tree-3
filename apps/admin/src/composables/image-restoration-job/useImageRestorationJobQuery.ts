import { useQuery } from '@tanstack/vue-query';
import { computed, type Ref } from 'vue';
import { useServices } from '@/plugins/services.plugin';

export const useImageRestorationJobQuery = (familyId: Ref<string>, jobId: Ref<string>) => {
  const services = useServices();
  const query = useQuery({
    queryKey: ['image-restoration-job', familyId, jobId],
    queryFn: async () => {
      if (!familyId.value || !jobId.value) {
        return undefined;
      }
      const result = await services.imageRestorationJob.getById(jobId.value, familyId.value);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    enabled: computed(() => !!familyId.value && !!jobId.value),
  });

  return {
    state: {
      imageRestorationJob: query.data,
      isLoading: query.isLoading,
      error: query.error,
    },
  };
};