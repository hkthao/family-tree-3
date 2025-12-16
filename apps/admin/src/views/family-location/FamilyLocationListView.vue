<template>
  <div data-testid="family-location-list-view">
    <FamilyLocationSearch @update:filters="handleFilterUpdate" />
    <FamilyLocationList
      :items="familyLocations"
      :total-items="totalItems"
      :loading="isLoadingFamilyLocations || isDeletingFamilyLocation"
      @update:options="handleListOptionsUpdate"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer()"
      :read-only="props.readOnly"
    >
    </FamilyLocationList>

    <!-- Add Family Location Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleClosed">
      <FamilyLocationAddView
        v-if="addDrawer"
        :family-id="props.familyId"
        @close="handleClosed"
        @saved="handleAdded"
      />
    </BaseCrudDrawer>

    <!-- Detail Family Location Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleClosed">
      <FamilyLocationDetailView
        v-if="selectedItemId && detailDrawer"
        :family-location-id="selectedItemId"
        @close="handleClosed"
        @edit-family-location="openEditDrawer"
      />
    </BaseCrudDrawer>

    <!-- Edit Family Location Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleClosed">
      <FamilyLocationEditView
        v-if="selectedItemId && editDrawer"
        :family-location-id="selectedItemId"
        @close="handleClosed"
        @saved="handleEdited"
      />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import type { FamilyLocation } from '@/types';
import { ref, watch, toRef } from 'vue';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import FamilyLocationSearch from '@/components/family-location/FamilyLocationSearch.vue';
import FamilyLocationList from '@/components/family-location/FamilyLocationList.vue';
import {
  useFamilyLocationsQuery,
  useDeleteFamilyLocationMutation,
  useFamilyLocationDataManagement,
  type FamilyLocationSearchCriteria,
} from '@/composables/family-location';
import { useI18n } from 'vue-i18n';

// Import drawer related components
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import FamilyLocationAddView from '@/views/family-location/FamilyLocationAddView.vue';
import FamilyLocationDetailView from '@/views/family-location/FamilyLocationDetailView.vue';
import FamilyLocationEditView from '@/views/family-location/FamilyLocationEditView.vue';

interface FamilyLocationListViewProps {
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<FamilyLocationListViewProps>();
const emit = defineEmits(['viewFamilyLocation', 'editFamilyLocation', 'createFamilyLocation', 'familyLocationDeleted']);
const { t } = useI18n();

const {
  filters,
  paginationOptions,
  setFilters,
  setPage,
  setItemsPerPage,
  setSortBy,
} = useFamilyLocationDataManagement(toRef(props, 'familyId'));

const { data: familyLocationsData, isLoading: isLoadingFamilyLocations, refetch } = useFamilyLocationsQuery(paginationOptions, filters);
const familyLocations = ref(familyLocationsData.value?.items || []);
const totalItems = ref(familyLocationsData.value?.totalItems || 0);

watch(familyLocationsData, (newData) => {
  familyLocations.value = newData?.items || [];
  totalItems.value = newData?.totalItems || 0;
}, { deep: true });

watch(() => props.familyId, (newFamilyId) => {
  setFilters({ familyId: newFamilyId });
  refetch();
});

const { mutate: deleteFamilyLocation, isPending: isDeletingFamilyLocation } = useDeleteFamilyLocationMutation();

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

// Drawer related logic
const {
  addDrawer,
  detailDrawer,
  editDrawer,
  selectedItemId,
  openAddDrawer,
  openDetailDrawer,
  openEditDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const handleAdded = () => {
  closeAllDrawers();
  refetch(); // Refetch the list after add
};

const handleEdited = () => {
  closeAllDrawers();
  refetch(); // Refetch the list after edit
};

const handleClosed = () => {
  closeAllDrawers();
};


const handleFilterUpdate = (criteria: FamilyLocationSearchCriteria) => {
  setFilters({
    ...filters,
    locationType: criteria.locationType,
    locationSource: criteria.locationSource,
  });
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  setPage(options.page);
  setItemsPerPage(options.itemsPerPage);
  setSortBy(options.sortBy as { key: string; order: 'asc' | 'desc' }[]);
};

const confirmDelete = async (familyLocationId: string) => {
  const familyLocationToDelete = familyLocations.value.find((f: FamilyLocation) => f.id === familyLocationId);
  if (!familyLocationToDelete) {
    showSnackbar(t('familyLocation.messages.notFound'), 'error');
    return;
  }
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyLocation.list.confirmDelete', { name: familyLocationToDelete.name }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });
  if (confirmed) {
    handleDeleteConfirm(familyLocationToDelete.id);
  }
};

const handleDeleteConfirm = (familyLocationId: string) => {
  deleteFamilyLocation(familyLocationId, {
    onSuccess: () => {
      showSnackbar(t('familyLocation.messages.deleteSuccess'), 'success');
      emit('familyLocationDeleted'); // This should trigger refetch in parent if needed
      closeAllDrawers(); // Close any open drawers after delete
      refetch(); // Refetch the list after delete
    },
    onError: (error) => {
      showSnackbar(error.message || t('familyLocation.messages.deleteError'), 'error');
    },
  });
};
</script>
