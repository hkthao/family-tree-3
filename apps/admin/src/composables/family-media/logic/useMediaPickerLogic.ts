import { ref, watch, computed, type Ref } from 'vue';
import { type MediaItem } from '@/types';
import { MediaType } from '@/types/enums';

type SelectionMode = 'single' | 'multiple';

interface UseMediaPickerLogicDeps {
  familyId: string;
  selectionMode: SelectionMode;
  initialSelection: string[] | string;
  initialSearchQuery: string;
  initialMediaType: string;
  mediaList: Ref<MediaItem[]>; // Changed to Ref
  setPage: (page: number) => void;
  setItemsPerPage: (items: number) => void;
  setSearchQuery: (query: string) => void;
  setMediaType: (type: string | MediaType | undefined) => void;
  setFamilyId: (familyId: string) => void;
  emit: (...args: any[]) => void;
  DEFAULT_ITEMS_PER_PAGE: number;
}

export function useMediaPickerLogic(deps: UseMediaPickerLogicDeps) {
  const itemsPerPage = ref(deps.DEFAULT_ITEMS_PER_PAGE);
  const internalSelectedIds = ref<string[]>([]);
  const localSearchQuery = ref(deps.initialSearchQuery);
  const localMediaType = ref<string | MediaType | undefined>(deps.initialMediaType);

  // Initialize internalSelectedIds based on initialSelection prop
  watch(() => deps.initialSelection, (newVal) => {
    if (deps.selectionMode === 'single') {
      internalSelectedIds.value = newVal ? [newVal as string] : [];
    } else {
      internalSelectedIds.value = Array.isArray(newVal) ? newVal : [];
    }
  }, { immediate: true, deep: true });

  // Update familyId in composable if prop changes
  watch(() => deps.familyId, (newFamilyId) => {
    deps.setFamilyId(newFamilyId);
  });

  // Update itemsPerPage in composable if local itemsPerPage changes
  watch(itemsPerPage, (newItemsPerPage) => {
    deps.setItemsPerPage(newItemsPerPage);
  });

  // Watch local search query and update composable
  watch(localSearchQuery, (newQuery) => {
    deps.setSearchQuery(newQuery || '');
  });

  // Watch local media type and update composable
  watch(localMediaType, (newType) => {
    deps.setMediaType(newType);
  });

  const isSelected = computed(() => (id: string) => internalSelectedIds.value.includes(id));

  const emitSelection = () => {
    const selectedMediaItems = deps.mediaList.value.filter(media => internalSelectedIds.value.includes(media.id)); // Access .value here

    if (deps.selectionMode === 'single') {
      deps.emit('update:selection', internalSelectedIds.value[0] || '');
      deps.emit('selected', selectedMediaItems[0] || null);
    } else {
      deps.emit('update:selection', internalSelectedIds.value);
      deps.emit('selected', selectedMediaItems);
    }
  };

  const toggleSelection = (id: string) => {
    if (deps.selectionMode === 'single') {
      internalSelectedIds.value = isSelected.value(id) ? [] : [id];
    } else {
      const index = internalSelectedIds.value.indexOf(id);
      if (index > -1) {
        internalSelectedIds.value.splice(index, 1);
      }
      else {
        internalSelectedIds.value.push(id);
      }
    }
    emitSelection();
  };

  const handlePageChange = (newPage: number) => {
    deps.setPage(newPage);
  };

  return {
    state: {
      itemsPerPage,
      localSearchQuery,
      localMediaType,
    },
    actions: {
      toggleSelection,
      handlePageChange,
      isSelected,
    },
  };
}
