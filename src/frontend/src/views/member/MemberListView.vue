<template>
  <div data-testid="member-list-view">
    <MemberSearch v-if="!props.hideSearch" @update:filters="handleFilterUpdate" />

    <MemberList :items="memberStore.items" :total-items="memberStore.totalItems" :loading="loading"
      :search="searchQuery" @update:search="handleSearchUpdate" @update:options="handleListOptionsUpdate"
      @view="navigateToDetailView" @edit="navigateToEditMember" @delete="confirmDelete" @create="navigateToCreateView"
      @ai-biography="navigateToAIBiography" @ai-create="openAiInputDialog" :read-only="props.readOnly" />
    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog :model-value="deleteConfirmDialog" :title="t('confirmDelete.title')" :message="t('member.list.confirmDelete', {
      fullName: memberToDelete?.fullName || '',
    })
      " @confirm="handleDeleteConfirm" @cancel="handleDeleteCancel" />

    <!-- AI Input Dialog -->
    <NLMemberPopup :model-value="aiInputDialog" @update:model-value="aiInputDialog = $event" @saved="handleAiSaved" />

    <!-- Edit Member Drawer -->

    <v-navigation-drawer v-model="editDrawer" location="right" temporary width="650">

      <MemberEditView v-if="selectedMemberId && editDrawer" :member-id="selectedMemberId" @close="handleMemberClosed"
        @saved="handleMemberSaved" />

    </v-navigation-drawer>

    <!-- Add Member Drawer -->
    <v-navigation-drawer v-model="addDrawer" location="right" temporary width="650">
      <MemberAddView v-if="addDrawer" :family-id="props.familyId" @close="handleMemberClosed"
        @saved="handleMemberSaved" />
    </v-navigation-drawer>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { MemberSearch, MemberList, NLMemberPopup } from '@/components/member';
import { ConfirmDeleteDialog } from '@/components/common';
import { useNotificationStore } from '@/stores/notification.store';
import MemberEditView from '@/views/member/MemberEditView.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import type { MemberFilter, Member } from '@/types';

interface MemberListViewProps {
  familyId?: string;
  readOnly?: boolean; // Add readOnly prop
  hideSearch?: boolean; // Add hideSearch prop
}

const props = defineProps<MemberListViewProps>();

const { t } = useI18n();
const memberStore = useMemberStore();
const { loading } = storeToRefs(memberStore);
const deleteConfirmDialog = ref(false); // Re-add deleteConfirmDialog
const memberToDelete = ref<Member | undefined>(undefined); // Add memberToDelete ref
const aiInputDialog = ref(false);
const searchQuery = ref('');
const editDrawer = ref(false); // Control visibility of the edit drawer
const addDrawer = ref(false); // Control visibility of the add drawer
const selectedMemberId = ref<string | null>(null); // Store the ID of the member being edited

const notificationStore = useNotificationStore();

const navigateToDetailView = (_member: Member) => {
  // Still navigate to detail view, as it's a separate full page view
  // router.push(`/member/detail/${member.id}`);
};

const navigateToCreateView = () => {
  addDrawer.value = true;
};

const navigateToEditMember = (member: Member) => {
  selectedMemberId.value = member.id;
  editDrawer.value = true;
};

const navigateToAIBiography = (_member: Member) => {
  // router.push(`/member/biography/${member.id}`);
};

const handleFilterUpdate = async (filters: MemberFilter) => {
  memberStore.filters = { ...filters, searchQuery: searchQuery.value, familyId: props.familyId };
  await memberStore._loadItems()
};

const handleSearchUpdate = async (search: string) => {
  searchQuery.value = search;
  memberStore.filters = { ...memberStore.filters, searchQuery: searchQuery.value, familyId: props.familyId };
  await memberStore._loadItems();
};

const handleListOptionsUpdate = async (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  await memberStore.setPage(options.page);
  await memberStore.setItemsPerPage(options.itemsPerPage);
  // Handle sorting
  if (options.sortBy && options.sortBy.length > 0) {
    memberStore.setSortBy(options.sortBy);
  } else {
    memberStore.setSortBy([]); // Clear sort if no sortBy is provided
  }
};

const confirmDelete = (member: Member) => {
  memberToDelete.value = member;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (memberToDelete.value) {
    await memberStore.deleteItem(memberToDelete.value.id);
    if (memberStore.error) {
      notificationStore.showSnackbar(
        t('member.messages.deleteError', { error: memberStore.error }),
        'error',
      );
    } else {
      notificationStore.showSnackbar(
        t('member.messages.deleteSuccess'),
        'success',
      );
    }
  }
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
  memberStore._loadItems(); // Refresh the member list after deleting
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

const openAiInputDialog = () => {
  aiInputDialog.value = true;
};

const handleAiSaved = () => {
  memberStore._loadItems(); // Refresh the member list after saving
};

const handleMemberSaved = () => {
  editDrawer.value = false;
  addDrawer.value = false;
  selectedMemberId.value = null;
  memberStore._loadItems(); // Refresh the member list after saving
};

const handleMemberClosed = () => {
  editDrawer.value = false;
  addDrawer.value = false;
  selectedMemberId.value = null;
};
onMounted(() => {
  if (props.familyId) {
    memberStore.getByFamilyId(props.familyId);
  } else {
    memberStore._loadItems();
  }
})

</script>
