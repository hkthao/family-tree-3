import { useQuery } from '@tanstack/vue-query';
import { computed, type Ref } from 'vue';
import { useServices } from '@/plugins/services.plugin';

export const useImageRestorationJobQuery = (familyId: Ref<string>, jobId: Ref<string>) => {
  const services = useServices();

  const isQueryEnabled = computed(() => {
    const enabled = !!familyId.value && !!jobId.value;
    console.log('useImageRestorationJobQuery - familyId:', familyId.value, 'jobId:', jobId.value, 'enabled:', enabled);
    return enabled;
  });

  const query = useQuery({
    queryKey: ['image-restoration-job', familyId, jobId],
    queryFn: async () => {
      // The `enabled` computed property already handles this check.
      // This check here is redundant if `enabled` is working as expected.
      // However, it doesn't hurt to keep it for robustness if `enabled` somehow fails.
      if (!familyId.value || !jobId.value) {
        console.warn('useImageRestorationJobQuery - queryFn called but familyId or jobId is missing.');
        return undefined;
      }
      const result = await services.imageRestorationJob.getById(jobId.value);
      if (result.ok) {
        console.log('useImageRestorationJobQuery - Data fetched successfully:', result.value);
        return result.value;
      }
      console.error('useImageRestorationJobQuery - Error fetching data:', result.error);
      throw result.error;
    },
    enabled: isQueryEnabled,
  });

  return {
    state: {
      imageRestorationJob: query.data,
      isLoading: query.isLoading,
      error: query.error,
    },
  };
};