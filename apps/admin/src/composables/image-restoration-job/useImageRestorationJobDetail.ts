import { type Ref } from 'vue';
import { useImageRestorationJobQuery } from './useImageRestorationJobQuery';

interface UseImageRestorationJobDetailOptions {
  familyId: Ref<string>;
  id: Ref<string>;
  onClose?: () => void;
}

export const useImageRestorationJobDetail = (options: UseImageRestorationJobDetailOptions) => {
  const { familyId, id, onClose } = options;

  const { state: { imageRestorationJob, isLoading, error } } = useImageRestorationJobQuery(familyId, id);

  const closeView = () => {
    onClose?.();
  };

  return {
    state: {
      imageRestorationJob,
      isLoading,
      error,
    },
    actions: {
      closeView,
    },
  };
};