<template>
  <div data-testid="family-location-list-view">
    <FamilyLocationSearch @update:search="handleSearchUpdate" />
    <FamilyLocationList
      :items="familyLocations"
      :total-items="totalItems"
      :loading="isLoadingFamilyLocations || isDeletingFamilyLocation"
      :search="searchQuery"
      @update:options="handleListOptionsUpdate"
      @view="$emit('viewFamilyLocation', $event)"
      @edit="$emit('editFamilyLocation', $event)"
      @delete="confirmDelete"
      @create="$emit('createFamilyLocation')"
      :read-only="props.readOnly"
    >
    </FamilyLocationList>
  </div>
</template>

<script setup lang="ts">
import type { FamilyLocation } from '@/types';
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQueryClient } from '@tanstack/vue-query';
import { useConfirmDialog, useGlobalSnackbar } from '@/composables';
import FamilyLocationSearch from '@/components/family-location/FamilyLocationSearch.vue';
import FamilyLocationList from '@/components/family-location/FamilyLocationList.vue';
import {
  useFamilyLocationsQuery,
  useDeleteFamilyLocationMutation,
  useFamilyLocationDataManagement,
} from '@/composables/family-location';

interface FamilyLocationListViewProps {
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<FamilyLocationListViewProps>();
const emit = defineEmits(['viewFamilyLocation', 'editFamilyLocation', 'createFamilyLocation', 'familyLocationDeleted']);
const { t } = useI18n();
const queryClient = useQueryClient();

const {
  searchQuery,
  paginationOptions,
  filters,
  setSearchQuery,
  setFilters,
  setPage,
  setItemsPerPage,
  setSortBy,
} = useFamilyLocationDataManagement(props.familyId);

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

const handleSearchUpdate = (search: string) => {
  setSearchQuery(search);
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
      emit('familyLocationDeleted');
    },
    onError: (error) => {
      showSnackbar(error.message || t('familyLocation.messages.deleteError'), 'error');
    },
  });
};
</script>
