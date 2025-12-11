<template>
  <div data-testid="family-media-list-view">
    <FamilyMediaSearch @update:filters="handleFilterUpdate" />
    <FamilyMediaList :items="familyMediaList" :total-items="totalItems" :loading="isLoading || isDeleting"
      @update:options="handleListOptionsUpdate" @view="openDetailDrawer" @edit="openEditDrawer" @delete="confirmDelete"
      @create="openAddDrawer()" />

    <!-- Add/Edit/Detail Drawers (similar to MemberListView) -->
    <!-- Detail Family Media Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <FamilyMediaDetailView v-if="selectedItemId && detailDrawer" :family-id="props.familyId"
        :family-media-id="selectedItemId" @close="handleDetailClosed" />
    </BaseCrudDrawer>

    <!-- Edit Family Media Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleMediaClosed">
      <FamilyMediaEditView v-if="selectedItemId && editDrawer" :family-id="props.familyId"
        :family-media-id="selectedItemId" @close="handleMediaClosed" @saved="handleMediaSaved" />
    </BaseCrudDrawer>

    <!-- Add Family Media Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMediaClosed">
      <FamilyMediaAddView v-if="addDrawer" :family-id="props.familyId" @close="handleMediaClosed"
        @saved="handleMediaSaved" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { nextTick, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import type { FamilyMediaFilter, ListOptions } from '@/types';
import { useFamilyMediaListQuery, useDeleteFamilyMediaMutation, useFamilyMediaListFilters } from '@/composables/family-media';
// Components
import FamilyMediaSearch from '@/components/family-media/FamilyMediaSearch.vue';
import FamilyMediaList from '@/components/family-media/FamilyMediaList.vue';
import FamilyMediaAddView from '@/views/family-media/FamilyMediaAddView.vue';
import FamilyMediaEditView from '@/views/family-media/FamilyMediaEditView.vue';
import FamilyMediaDetailView from '@/views/family-media/FamilyMediaDetailView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';

const props = defineProps<{
  familyId: string;
}>();

const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const familyIdRef = toRef(props, 'familyId');

const familyMediaListFiltersComposable = useFamilyMediaListFilters();
const { filters, listOptions } = familyMediaListFiltersComposable;
const { setItemsPerPage, setPage, setSortBy, setFilters } = familyMediaListFiltersComposable;

const { familyMediaList, totalItems, isLoading, refetch } = useFamilyMediaListQuery(familyIdRef, filters, listOptions);
const { mutate: deleteFamilyMedia, isPending: isDeleting } = useDeleteFamilyMediaMutation();

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const handleFilterUpdate = (newFilters: FamilyMediaFilter) => {
  setFilters(newFilters);
};

const handleListOptionsUpdate = async (options: ListOptions) => {
  await nextTick();
  setPage(options.page || 1);
  setItemsPerPage(options.itemsPerPage || 10);
  setSortBy(options.sortBy);
};

const confirmDelete = async (familyMediaId: string) => {
  const media = familyMediaList.value.find(m => m.id === familyMediaId);
  if (!media) {
    showSnackbar(t('familyMedia.messages.notFound'), 'error');
    return;
  }
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyMedia.list.confirmDelete', { fileName: media.fileName || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });
  if (confirmed) {
    deleteFamilyMedia({ familyId: props.familyId, id: media.id }, {
      onSuccess: () => {
        showSnackbar(t('familyMedia.messages.deleteSuccess'), 'success');
        refetch(); // Refetch the list after successful deletion
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyMedia.messages.deleteError'), 'error');
      },
    });
  }
};

const handleMediaSaved = () => {
  closeAllDrawers();
  refetch(); // Refetch the list after add/edit/delete
};

const handleMediaClosed = () => {
  closeAllDrawers();
};

const handleDetailClosed = () => {
  closeAllDrawers();
};
</script>