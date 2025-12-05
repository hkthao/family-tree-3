<template>
  <div data-testid="member-face-list-view">
    <MemberFaceSearch @update:filters="handleFilterUpdate" />

    <MemberFaceList :items="memberFaceStore.list.items" :total-items="memberFaceStore.list.totalItems"
      :loading="list.loading" :search="searchQuery" @update:options="handleListOptionsUpdate" @view="openDetailDrawer"
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
import { onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useCrudDrawer } from '@/composables/useCrudDrawer';
import { useMemberFaceStore } from '@/stores/member-face.store';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import type { MemberFace, MemberFaceFilter } from '@/types'; // Import MemberFaceFilter
import MemberFaceList from '@/components/member-face/MemberFaceList.vue';
import MemberFaceAddView from '@/views/member-face/MemberFaceAddView.vue';
import MemberFaceDetailView from '@/views/member-face/MemberFaceDetailView.vue';
import MemberFaceSearch from '@/components/member-face/MemberFaceSearch.vue'; // NEW

interface MemberFaceListViewProps {
  memberId?: string;
  familyId?: string;
}
const props = defineProps<MemberFaceListViewProps>();
const { t } = useI18n();
const memberFaceStore = useMemberFaceStore();
const { list } = storeToRefs(memberFaceStore);
const { showSnackbar } = useGlobalSnackbar();
const { showConfirmDialog } = useConfirmDialog();
const {
  addDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const searchQuery = ref(''); // NEW

const loadMemberFaces = async () => {
  // Create a base filter from props (route-based)
  const baseFilters: MemberFaceFilter = {
    memberId: props.memberId,
    familyId: props.familyId,
  };

  // Merge with existing filters from the store, prioritizing store filters
  // This means if MemberFaceSearch sets memberId, it overrides props.memberId
  memberFaceStore.list.filters = {
    ...baseFilters, // Apply initial route filters
    ...memberFaceStore.list.filters, // Apply user-set filters from search component
    searchQuery: searchQuery.value, // Ensure text search is included
  };
  await memberFaceStore._loadItems();
};

const handleFilterUpdate = (filters: MemberFaceFilter) => {
  memberFaceStore.list.filters = {
    ...memberFaceStore.list.filters,
    ...filters,
  };
  memberFaceStore.list.options.page = 1; // Reset page to 1 when filters change
  loadMemberFaces();
};

const handleSearchUpdate = (search: string) => { // NEW
  searchQuery.value = search;
  memberFaceStore.list.filters.searchQuery = search;
  memberFaceStore.list.options.page = 1; // Reset page on text search
  loadMemberFaces();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  memberFaceStore.setListOptions(options);
  loadMemberFaces();
};

const confirmDelete = async (memberFace: MemberFace) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('memberFace.list.confirmDelete', { faceId: memberFace.faceId }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });
  if (confirmed) {
    await handleDeleteConfirm(memberFace);
  }
};

const handleDeleteConfirm = async (memberFace: MemberFace) => {
  if (memberFace) {
    await memberFaceStore.deleteItem(memberFace.id);
    if (memberFaceStore.delete.error) {
      showSnackbar(
        memberFaceStore.delete.error.message || t('memberFace.messages.deleteError'),
        'error',
      );
    } else {
      showSnackbar(t('memberFace.messages.deleteSuccess'), 'success');
    }
  }
  loadMemberFaces();
};

const handleMemberFaceSaved = () => {
  closeAllDrawers();
  loadMemberFaces();
};

const handleMemberFaceClosed = () => {
  closeAllDrawers();
};

const handleDetailClosed = () => {
  closeAllDrawers();
};

onMounted(() => {
  loadMemberFaces();
});

watch([() => props.memberId, () => props.familyId], () => {
  loadMemberFaces();
});
</script>