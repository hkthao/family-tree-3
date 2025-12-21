<template>
  <div data-testid="family-media-list-view">
    <FamilyMediaSearch @update:filters="handleFilterUpdate" />
    <FamilyMediaList
      :items="familyMediaList"
      :total-items="totalItems"
      :loading="isLoading || isDeleting"
      :search="filters.searchQuery"
      :items-per-page="itemsPerPage"
      :sort-by="sortBy"
      @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate"
      @view="openDetailDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer()"
      @add-link="openAddLinkDrawer()"
      :allow-add="allowAdd"
      :allow-edit="allowEdit"
      :allow-delete="allowDelete"
      :allow-add-link="allowAdd"
    />

    <!-- Add/Edit/Detail Drawers (similar to MemberListView) -->
    <!-- Detail Family Media Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <FamilyMediaDetailView v-if="selectedItemId && detailDrawer" :family-id="props.familyId"
        :family-media-id="selectedItemId" @close="handleDetailClosed" />
    </BaseCrudDrawer>

    <!-- Add Family Media Drawer -->
    <BaseCrudDrawer v-if="allowAdd" v-model="addDrawer" @close="handleMediaClosed">
      <FamilyMediaAddView v-if="addDrawer" :family-id="props.familyId" @close="handleMediaClosed"
        @saved="handleMediaSaved" />
    </BaseCrudDrawer>

    <!-- Add Family Media From Link Drawer -->
    <BaseCrudDrawer v-if="allowAdd" v-model="addLinkDrawer" @close="handleMediaClosed">
      <FamilyMediaAddLinkView v-if="addLinkDrawer" :family-id="props.familyId" @close="handleMediaClosed"
        @saved="handleMediaSaved" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { nextTick, toRef, computed, ref } from 'vue'; // Added ref
import { useCrudDrawer } from '@/composables';
import type { FamilyMediaFilter, ListOptions } from '@/types';
import { useFamilyMediaListQuery, useDeleteFamilyMediaMutation, useFamilyMediaListFilters, useFamilyMediaDeletion } from '@/composables';
import { useAuth } from '@/composables'; // Import useAuth
// Components
import FamilyMediaSearch from '@/components/family-media/FamilyMediaSearch.vue';
import FamilyMediaList from '@/components/family-media/FamilyMediaList.vue';
import FamilyMediaAddView from '@/views/family-media/FamilyMediaAddView.vue';
import FamilyMediaDetailView from '@/views/family-media/FamilyMediaDetailView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import FamilyMediaAddLinkView from '@/views/family-media/FamilyMediaAddLinkView.vue'; // New Import

const props = defineProps<{
  familyId: string;
}>();

const { state } = useAuth(); // Import useAuth
const allowAdd = computed(() => state.isAdmin.value || state.isFamilyManager.value(props.familyId));
const allowEdit = computed(() => state.isAdmin.value || state.isFamilyManager.value(props.familyId)); // Edit currently not implemented for media, but for future proofing
const allowDelete = computed(() => state.isAdmin.value || state.isFamilyManager.value(props.familyId));

const familyIdRef = toRef(props, 'familyId');
const familyMediaListFiltersComposable = useFamilyMediaListFilters(familyIdRef);
const { filters, page, itemsPerPage, sortBy, setItemsPerPage, setPage, setSortBy, setFilters } = familyMediaListFiltersComposable;
const { familyMediaList, totalItems, isLoading, refetch } = useFamilyMediaListQuery(filters, page, itemsPerPage, sortBy);
const { mutateAsync: deleteFamilyMediaMutation } = useDeleteFamilyMediaMutation();

const { isDeleting, confirmAndDelete } = useFamilyMediaDeletion({
  familyId: familyIdRef,
  deleteMutation: deleteFamilyMediaMutation,
  successMessageKey: 'familyMedia.messages.deleteSuccess',
  errorMessageKey: 'familyMedia.messages.deleteError',
  confirmationTitleKey: 'confirmDelete.title',
  confirmationMessageKey: 'familyMedia.list.confirmDelete',
  refetchList: refetch,
});

const {
    addDrawer,
    detailDrawer,
    selectedItemId,
    openAddDrawer,
    openDetailDrawer,
    closeAllDrawers,
  } = useCrudDrawer<string>();

const addLinkDrawer = ref(false); // New state for addLinkDrawer

const openAddLinkDrawer = () => {
  addLinkDrawer.value = true;
};

const handleFilterUpdate = (newFilters: FamilyMediaFilter) => {
  setFilters(newFilters);
};

const handleSearchUpdate = (newSearchQuery: string) => {
  setFilters({ ...filters.value, searchQuery: newSearchQuery });
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
    // Optionally show a snackbar here that the media was not found
    return;
  }
  await confirmAndDelete(familyMediaId, media.fileName || '');
};

const handleMediaSaved = () => {
  closeAllDrawers();
  addLinkDrawer.value = false; // Close addLinkDrawer as well
  refetch(); // Refetch the list after add/edit/delete
};

const handleMediaClosed = () => {
  closeAllDrawers();
  addLinkDrawer.value = false; // Close addLinkDrawer as well
};

const handleDetailClosed = () => {
  closeAllDrawers();
};
</script>
