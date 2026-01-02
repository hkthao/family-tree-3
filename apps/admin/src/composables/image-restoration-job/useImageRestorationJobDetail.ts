import { reactive, type Ref } from 'vue';
import { useImageRestorationJobQuery } from './useImageRestorationJobQuery';

interface UseImageRestorationJobDetailOptions {
  familyId: Ref<string>;
  imageRestorationJobId: Ref<string>;
  onClose?: () => void;
}

export const useImageRestorationJobDetail = (options: UseImageRestorationJobDetailOptions) => {
  const { familyId, imageRestorationJobId, onClose } = options;

  const { state: { imageRestorationJob, isLoading, error } } = useImageRestorationJobQuery(familyId, imageRestorationJobId);

  const closeView = () => {
    onClose?.();
  };

  return {
    state: reactive({
      imageRestorationJob,
      isLoading,
      error,
    }),
    actions: {
      closeView,
    },
  };
};