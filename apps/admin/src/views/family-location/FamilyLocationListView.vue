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

    <!-- Map Picker Drawer -->
    <BaseCrudDrawer v-model="mapDrawer" :width="700" :hide-overlay="false" :location="'right'" @close="closeMapDrawer">
       <MapView
        v-if="mapDrawer"
        :initial-coordinates="initialMapCoordinates"
        @update:coordinates="handleMapCoordinatesSelected"
        @close="closeMapDrawer"
      />
    </BaseCrudDrawer>

    <!-- Add Family Location Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleClosed">
      <FamilyLocationAddView
        ref="familyLocationAddViewRef"
        v-if="addDrawer"
        :family-id="props.familyId"
        @close="handleClosed"
        @saved="handleAdded"
        @open-map-picker="handleOpenMapPicker"
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
        ref="familyLocationEditViewRef"
        v-if="selectedItemId && editDrawer"
        :family-location-id="selectedItemId"
        @close="handleClosed"
        @saved="handleEdited"
        @open-map-picker="handleOpenMapPicker"
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
import MapView from '@/views/map/MapView.vue'; // Import MapView

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

// CRUD Drawer related logic for Add, Detail, Edit
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

// Map Drawer related logic
const {
  addDrawer: mapDrawer, // Use alias for map drawer
  openAddDrawer: openMapDrawer,
  closeAllDrawers: closeMapDrawer,
} = useCrudDrawer<string>();

const initialMapCoordinates = ref<{ latitude?: number; longitude?: number }>({});

// Refs for the Add/Edit views to call their exposed methods
const familyLocationAddViewRef = ref<InstanceType<typeof FamilyLocationAddView> | null>(null);
const familyLocationEditViewRef = ref<InstanceType<typeof FamilyLocationEditView> | null>(null);


const handleOpenMapPicker = (coordinates: { latitude?: number; longitude?: number }) => {
  initialMapCoordinates.value = coordinates;
  openMapDrawer();
};

const handleMapCoordinatesSelected = (coords: { latitude: number; longitude: number }) => {
  if (addDrawer.value && familyLocationAddViewRef.value) {
    familyLocationAddViewRef.value.setCoordinates(coords);
  } else if (editDrawer.value && familyLocationEditViewRef.value) {
    familyLocationEditViewRef.value.setCoordinates(coords);
  }
  closeMapDrawer();
};


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
