import { useMemoryItemQuery } from '@/composables/memory-item';

interface UseMemoryItemDetailOptions {
  familyId: string;
  memoryItemId: string;
  onClose: () => void;
}

export function useMemoryItemDetail(options: UseMemoryItemDetailOptions) {
  const { familyId, memoryItemId, onClose } = options;

  const { data: memoryItem, isLoading, error } = useMemoryItemQuery(
    familyId,
    memoryItemId,
  );

  const closeView = () => {
    onClose();
  };

  return {
    memoryItem,
    isLoading,
    error,
    closeView,
  };
}