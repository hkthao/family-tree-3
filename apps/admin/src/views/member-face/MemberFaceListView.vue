<template>
  <div data-testid="member-face-list-view">
    <MemberFaceSearch @update:filters="handleFilterUpdate" />

    <MemberFaceList :items="memberFaces" :total-items="totalItems" :loading="queryLoading || isDeleting" :search="searchQuery"
      :items-per-page="itemsPerPage" :sortBy="sortBy" @update:options="handleListOptionsUpdate" @view="openDetailDrawer"
      @delete="confirmDelete" @create="openAddDrawer()" @update:search="handleSearchUpdate" />

    <!-- Add MemberFace Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMemberFaceClosed">
      <MemberFaceAddView v-if="addDrawer" :member-id="props.memberId" :family-id="props.familyId"
        @close="handleMemberFaceClosed" @saved="handleMemberFaceSaved" />
    </BaseCrudDrawer>
    <!-- Detail MemberFace Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <MemberFaceDetailView v-if="selectedItemId && detailDrawer" :member-face-id="selectedItemId"
        @close="handleDetailClosed" />
    </BaseCrudDrawer>
  </div>
</template>
<script setup lang="ts">
import { onMounted, watch, computed } from 'vue';
import { useCrudDrawer } from '@/composables';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import type { MemberFace, MemberFaceFilter, ListOptions, FilterOptions } from '@/types';
import MemberFaceList from '@/components/member-face/MemberFaceList.vue';
import MemberFaceAddView from '@/views/member-face/MemberFaceAddView.vue';
import MemberFaceDetailView from '@/views/member-face/MemberFaceDetailView.vue';
import MemberFaceSearch from '@/components/member-face/MemberFaceSearch.vue';
import { useMemberFaceListFilters, useMemberFacesQuery, useDeleteMemberFaceMutation, useMemberFaceDeletion } from '@/composables';

interface MemberFaceListViewProps {
  memberId?: string;
  familyId?: string;
}
const props = defineProps<MemberFaceListViewProps>();
const {
  addDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const memberFaceListFiltersComposables = useMemberFaceListFilters();
const {
  state: { searchQuery, page, itemsPerPage, sortBy, filters },
  actions: { setPage, setItemsPerPage, setSortBy, setSearchQuery, setFilters },
} = memberFaceListFiltersComposables;

  const listOptions = computed<ListOptions>(() => ({
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value.map((s: { key: string; order: 'asc' | 'desc' }) => ({ key: s.key, order: s.order as 'asc' | 'desc' })),
  }));
// Create a reactive filter object to pass to the query
const queryFilters = computed<FilterOptions>(() => ({
  ...filters,
  searchQuery: searchQuery.value,
}));

const { memberFaces, totalItems, queryLoading, refetch } = useMemberFacesQuery(listOptions, queryFilters);
const { mutateAsync: deleteMemberFaceMutation } = useDeleteMemberFaceMutation();

const { state: { isDeleting }, actions: { confirmAndDelete } } = useMemberFaceDeletion({
  deleteMutation: deleteMemberFaceMutation,
  successMessageKey: 'memberFace.messages.deleteSuccess',
  errorMessageKey: 'memberFace.messages.deleteError',
  confirmationTitleKey: 'confirmDelete.title',
  confirmationMessageKey: 'memberFace.list.confirmDelete',
  refetchList: refetch,
});

const handleFilterUpdate = (newFilters: MemberFaceFilter) => {
  setFilters(newFilters);
};

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

const confirmDelete = async (memberFace: MemberFace) => {
  await confirmAndDelete(memberFace.id, memberFace.faceId);
};

const handleMemberFaceSaved = () => {
  closeAllDrawers();
  refetch(); // Refetch the list after successful save
};

const handleMemberFaceClosed = () => {
  closeAllDrawers();
};

const handleDetailClosed = () => {
  closeAllDrawers();
};

onMounted(() => {
  // Initialize filters from props
  setFilters({
    memberId: props.memberId,
    familyId: props.familyId,
  });
});

watch([() => props.memberId, () => props.familyId], ([newMemberId, newFamilyId]) => {
  setFilters({
    memberId: newMemberId,
    familyId: newFamilyId,
  });
});
</script>