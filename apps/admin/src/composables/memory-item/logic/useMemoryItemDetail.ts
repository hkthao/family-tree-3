import { useMemoryItemQuery } from '@/composables';

interface UseMemoryItemDetailOptions {
  familyId: string;
  memoryItemId: string;
  onClose: () => void;
}

export function useMemoryItemDetail(options: UseMemoryItemDetailOptions) {
  const { memoryItemId, onClose } = options;

  const { data: memoryItem, isLoading, error } = useMemoryItemQuery(
    memoryItemId,
  );

  const closeView = () => {
    onClose();
  };

  return {
    state: {
      memoryItem,
      isLoading,
      error,
    },
    actions: {
      closeView,
    },
  };
}