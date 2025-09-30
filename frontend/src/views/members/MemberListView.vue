<template>
    <MemberSearch @update:filters="handleFilterUpdate" />

    <MemberList
      :items="memberStore.items"
      :total-items="memberStore.totalItems"
      :loading="loading"
      @update:options="handleListOptionsUpdate"
      @view="navigateToDetailView"
      @edit="navigateToEditMember"
      @delete="confirmDelete"
      @create="navigateToCreateView"
    />

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
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types/family';
import type { MemberFilter } from '@/types/family';
import { MemberSearch, MemberList } from '@/components/members';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import { useNotificationStore } from '@/stores/notification.store';
import { useRouter } from 'vue-router';

const { t } = useI18n();
const router = useRouter();
const memberStore = useMemberStore();
const { loading, currentPage } = storeToRefs(memberStore);
const currentFilters = ref<MemberFilter>({});
const deleteConfirmDialog = ref(false); // Re-add deleteConfirmDialog
const memberToDelete = ref<Member | undefined>(undefined); // Add memberToDelete ref

const notificationStore = useNotificationStore();

// Function Declarations (moved to top)
const loadMembers = () => {
  memberStore.searchItems(currentFilters.value);
};

const navigateToDetailView = (member: Member) => {
  router.push(`/members/detail/${member.id}`);
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
  loadMembers();
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
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

onMounted(async () => {
  await loadMembers();
});
</script>
