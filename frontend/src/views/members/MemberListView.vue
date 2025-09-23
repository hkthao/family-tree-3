<template>
  <v-container fluid>
    <MemberSearch @update:filters="handleFilterUpdate" />

    <MemberList
      :members="paginatedItems"
      :total-members="filteredItems.length"
      :loading="loading"
      @update:options="handleListOptionsUpdate"
      @view="openViewDialog"
      @edit="navigateToEditMember"
      @delete="confirmDelete"
      @create="navigateToCreateView"
    />

    <v-dialog v-model="viewDialog" max-width="800px">
      <MemberForm
        v-if="selectedMemberForView"
        :initial-member-data="selectedMemberForView"
        :read-only="true"
        :title="t('member.form.title')"
        @close="closeViewDialog"
      />
    </v-dialog>

    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
      :title="t('confirmDelete.title')"
      :message="
        t('member.list.confirmDelete', {
          fullName: memberToDelete?.fullName || '',
        })
      "
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <!-- Global Snackbar -->
    <v-snackbar
      v-if="notificationStore.snackbar"
      v-model="notificationStore.snackbar.show"
      :color="notificationStore.snackbar.color"
      timeout="3000"
    >
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { useFamilyStore } from '@/stores/family.store';
import type { Member } from '@/types/member';
import type { MemberFilter } from '@/services/member/member.service.interface';
import MemberSearch from '@/components/members/MemberSearch.vue';
import MemberList from '@/components/members/MemberList.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import MemberForm from '@/components/members/MemberForm.vue';
import { useNotificationStore } from '@/stores/notification.store';
import { useRouter } from 'vue-router';

const { t } = useI18n();
const router = useRouter();
const memberStore = useMemberStore();
const { loading, currentPage, paginatedItems, filteredItems } =
  storeToRefs(memberStore);
const currentFilters = ref<MemberFilter>({});
const deleteConfirmDialog = ref(false); // Re-add deleteConfirmDialog
const memberToDelete = ref<Member | undefined>(undefined); // Add memberToDelete ref
const viewDialog = ref(false);
const selectedMemberForView = ref<Member | null>(null);

const notificationStore = useNotificationStore();

// Function Declarations (moved to top)
const loadMembers = () => {
  memberStore.searchItems(currentFilters.value);
};

const loadAllMembers = async () => {
  await memberStore.fetchItems(); // Fetch all members
};

const openViewDialog = (member: Member) => {
  selectedMemberForView.value = member;
  viewDialog.value = true;
};

const closeViewDialog = () => {
  viewDialog.value = false;
  selectedMemberForView.value = null;
};

const navigateToCreateView = () => {
  router.push('/members/add');
};

const navigateToEditMember = (member: Member) => {
  router.push(`/members/edit/${member.id}`);
};

const handleFilterUpdate = (filters: MemberFilter) => {
  currentFilters.value = filters;
  currentPage.value = 1; // Reset to first page on filter change
  loadMembers();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
}) => {
  memberStore.setPage(options.page);
  memberStore.setItemsPerPage(options.itemsPerPage);
};

const confirmDelete = (member: Member) => {
  memberToDelete.value = member;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (memberToDelete.value) {
    try {
      await memberStore.deleteItem(memberToDelete.value.id);
      notificationStore.showSnackbar(
        t('member.messages.deleteSuccess'),
        'success',
      );
      await memberStore.fetchItems(); // Reload members after deletion
    } catch (error) {
      notificationStore.showSnackbar(t('member.messages.deleteError'), 'error');
    }
  }
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

onMounted(async () => {
  await loadMembers();
  await loadAllMembers();
});
</script>
