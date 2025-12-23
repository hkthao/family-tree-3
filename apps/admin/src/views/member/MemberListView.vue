<template>
  <div data-testid="member-list-view">
    <MemberSearch @update:filters="handleFilterUpdate" />
    <MemberList :items="members" :total-items="totalItems" :loading="isLoadingMembers || isDeletingMember"
      :search="searchQuery" @update:search="handleSearchUpdate" @update:options="handleListOptionsUpdate"
      @view="openDetailDrawer" @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer()" :read-only="props.readOnly"
      :allow-add="allowAdd" :allow-edit="allowEdit" :allow-delete="allowDelete">
    </MemberList>
    <!-- Edit Member Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleMemberClosed">
      <MemberEditView v-if="selectedItemId && editDrawer" :member-id="selectedItemId" @close="handleMemberClosed"
        @saved="handleMemberSaved"
        :allow-save="allowEdit" />
    </BaseCrudDrawer>
    <!-- Add Member Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMemberClosed">
      <MemberAddView v-if="addDrawer" :family-id="props.familyId"
        @close="handleMemberClosed" @saved="handleMemberSaved"
        :allow-save="allowAdd" />
    </BaseCrudDrawer>
    <!-- Detail Member Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <MemberDetailView v-if="selectedItemId && detailDrawer" :member-id="selectedItemId" @close="handleDetailClosed"
        @edit-member="openEditDrawer" />
    </BaseCrudDrawer>


  </div>
</template>
<script setup lang="ts">
import { MemberSearch, MemberList } from '@/components/member';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import MemberEditView from '@/views/member/MemberEditView.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import MemberDetailView from '@/views/member/MemberDetailView.vue';
import NLEditorView from '@/views/member/NLEditorView.vue';
import type { MemberFilter } from '@/types';
import { useI18n } from 'vue-i18n';
import { ref, watch, computed } from 'vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { removeDiacritics } from '@/utils/string.utils';
import { useMembersQuery, useDeleteMemberMutation, useMemberDataManagement } from '@/composables';
import { useQueryClient } from '@tanstack/vue-query'; // Import useQueryClient
import { useAuth } from '@/composables'; // Import useAuth

interface MemberListViewProps {
  familyId: string;
  readOnly?: boolean;
}
const props = defineProps<MemberListViewProps>();
const { t } = useI18n();
const queryClient = useQueryClient(); // Initialize useQueryClient
const { state } = useAuth(); // Import useAuth

const allowAdd = computed(() => !props.readOnly && (state.isAdmin.value || state.isFamilyManager.value(props.familyId)));
const allowEdit = computed(() => !props.readOnly && (state.isAdmin.value || state.isFamilyManager.value(props.familyId)));
const allowDelete = computed(() => !props.readOnly && (state.isAdmin.value || state.isFamilyManager.value(props.familyId)));

const {
  state: { searchQuery, paginationOptions, filters },
  actions: { setSearchQuery, setFilters, setPage, setItemsPerPage, setSortBy },
} = useMemberDataManagement(props.familyId);

const { data: membersData, isLoading: isLoadingMembers, refetch } = useMembersQuery(paginationOptions, filters);
const members = ref(membersData.value?.items || []);
const totalItems = ref(membersData.value?.totalItems || 0);

watch(membersData, (newData) => {
  members.value = newData?.items || [];
  totalItems.value = newData?.totalItems || 0;
}, { deep: true });

watch(() => props.familyId, (newFamilyId) => {
  setFilters({ familyId: newFamilyId });
  refetch();
});

const { mutate: deleteMember, isPending: isDeletingMember } = useDeleteMemberMutation();

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

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const handleFilterUpdate = (newFilters: MemberFilter) => {
  setFilters(newFilters);
};

const handleSearchUpdate = (search: string) => {
  const processedSearch = removeDiacritics(search); 
  setSearchQuery(processedSearch); // setSearchQuery handles debounce and updates filters
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

const confirmDelete = async (memberId: string) => {
  const memberToDelete = members.value.find(m => m.id === memberId);
  if (!memberToDelete) {
    showSnackbar(t('member.messages.notFound'), 'error');
    return;
  }
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('member.list.confirmDelete', { fullName: memberToDelete.fullName || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });
  if (confirmed) {
    handleDeleteConfirm(memberToDelete.id);
  }
};

const handleDeleteConfirm = (memberId: string) => {
  deleteMember(memberId, {
    onSuccess: () => {
      showSnackbar(t('member.messages.deleteSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['members', 'list'] }); // Invalidate list to refetch
    },
    onError: (error) => {
      showSnackbar(error.message || t('member.messages.deleteError'), 'error');
    },
  });
};

const handleMemberSaved = () => {
  closeAllMemberDrawers(); 
  queryClient.invalidateQueries({ queryKey: ['members', 'list'] }); // Invalidate list to refetch
};

const handleMemberClosed = () => {
  closeAllMemberDrawers(); 
};

const handleDetailClosed = () => {
  closeAllMemberDrawers(); 
};

const closeAllMemberDrawers = () => {
  closeAllDrawers();
};

// Initial load is handled by useMembersQuery, no need for onMounted load
</script>