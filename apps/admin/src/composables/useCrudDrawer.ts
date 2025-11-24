import { ref } from 'vue';

export function useCrudDrawer<T = string>() {
  const addDrawer = ref(false);
  const editDrawer = ref(false);
  const detailDrawer = ref(false);
  const selectedItemId = ref<T | null>(null);
  const initialData = ref<any | null>(null); // For passing initial data to add/edit forms

  const openAddDrawer = (data: any = null) => {
    initialData.value = data;
    addDrawer.value = true;
  };

  const openEditDrawer = (id: T, data: any = null) => {
    selectedItemId.value = id;
    initialData.value = data;
    editDrawer.value = true;
  };

  const openDetailDrawer = (id: T, data: any = null) => {
    selectedItemId.value = id;
    initialData.value = data;
    detailDrawer.value = true;
  };

  const closeAddDrawer = () => {
    addDrawer.value = false;
    initialData.value = null;
  };

  const closeEditDrawer = () => {
    editDrawer.value = false;
    selectedItemId.value = null;
    initialData.value = null;
  };

  const closeDetailDrawer = () => {
    detailDrawer.value = false;
    selectedItemId.value = null;
    initialData.value = null;
  };

  // Generic close function for any drawer
  const closeAllDrawers = () => {
    addDrawer.value = false;
    editDrawer.value = false;
    detailDrawer.value = false;
    selectedItemId.value = null;
    initialData.value = null;
  };

  return {
    addDrawer,
    editDrawer,
    detailDrawer,
    selectedItemId,
    initialData,
    openAddDrawer,
    openEditDrawer,
    openDetailDrawer,
    closeAddDrawer,
    closeEditDrawer,
    closeDetailDrawer,
    closeAllDrawers,
  };
}
