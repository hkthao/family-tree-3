<template>
  <div data-testid="family-link-requests-list-view">
    <!-- Removed v-card toolbar, it's now inside FamilyLinkRequestList -->
    <FamilyLinkRequestList :items="familyLinkRequestStore.list.items"
      :total-items="familyLinkRequestStore.list.totalItems" :loading="familyLinkRequestStore.list.loading"
      :read-only="readOnly" :search="searchQuery" @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate" @view="openDetailDrawer" @edit="openEditDrawer" @delete="confirmDelete"
      @create="openAddDrawer" @approve="confirmApprove" @reject="confirmReject" />

    <!-- Add Request Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('familyLinkRequest.form.addTitle')" icon="mdi-plus"
      @close="closeAddDrawer">
      <FamilyLinkRequestAddView v-if="addDrawer && familyId" :family-id="familyId" @close="closeAddDrawer"
        @saved="handleRequestSaved" />
    </BaseCrudDrawer>

    <!-- Detail Request Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('familyLinkRequest.detail.title')" icon="mdi-information-outline"
      @close="closeDetailDrawer">
      <FamilyLinkRequestDetailView v-if="selectedItemId && detailDrawer" :family-link-request-id="selectedItemId"
        :family-id="familyId" @close="closeDetailDrawer" @edit-family-link-request="openEditDrawer" />
    </BaseCrudDrawer>

    <!-- Approve/Reject Dialog -->
    <ApproveRejectFamilyLinkRequestDialog :show="approveRejectDialog" :action-type="dialogActionType"
      @update:show="approveRejectDialog = $event" @confirm="handleApproveRejectConfirm" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyLinkRequestStore } from '@/stores/familyLinkRequest.store';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { Result } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // New import
import { useCrudDrawer } from '@/composables/useCrudDrawer'; // New import

// New imports for the views
import { FamilyLinkRequestList, ApproveRejectFamilyLinkRequestDialog } from '@/components/family-link-requests'; // NEW IMPORT
import FamilyLinkRequestAddView from '@/views/family-link-requests/FamilyLinkRequestAddView.vue';

import FamilyLinkRequestDetailView from '@/views/family-link-requests/FamilyLinkRequestDetailView.vue';

interface FamilyLinkRequestsListViewProps {
  familyId: string;
}

const props = defineProps<FamilyLinkRequestsListViewProps>();

const { t } = useI18n();
const familyLinkRequestStore = useFamilyLinkRequestStore();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const familyId = computed(() => props.familyId);
const searchQuery = ref(''); // NEW: Local search query state

const {
  addDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeDetailDrawer,
  closeAllDrawers, // To close all drawers after save/delete
} = useCrudDrawer<string>();

const readOnly = ref(false); // Can be made reactive if needed

const approveRejectDialog = ref(false);
const dialogActionType = ref<'approve' | 'reject'>('approve');
const requestIdForDialog = ref<string | null>(null);

const loadRequests = async () => {
  if (familyId.value) {
    familyLinkRequestStore.list.filters.searchQuery = searchQuery.value; // Apply search query
    await familyLinkRequestStore._loadItems(familyId.value);
  }
};

const handleListOptionsUpdate = async (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  if (familyId.value) {
    await nextTick(); // Ensure DOM is updated before store changes for data-table
    familyLinkRequestStore.setListOptions(familyId.value, options);
  }
};

const handleSearchUpdate = async (search: string) => { // NEW: Handle search emit
  const processedSearch = search.trim(); // No diacritics removal for now
  searchQuery.value = processedSearch;
  familyLinkRequestStore.list.filters.searchQuery = processedSearch;
  await familyLinkRequestStore._loadItems(familyId.value);
};

const confirmDelete = async (id: string) => {
  const confirmed = await showConfirmDialog({
    title: t('familyLinkRequest.list.confirmDelete.title'),
    message: t('familyLinkRequest.list.confirmDelete.message'),
    confirmText: t('common.delete'),
    confirmColor: 'error',
  });

  if (confirmed && familyId.value) {
    const result = await familyLinkRequestStore.deleteRequest(id, familyId.value);
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.list.messages.deleteSuccess'), 'success');
      loadRequests();
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.list.messages.deleteError'), 'error');
    }
  }
};

const confirmApprove = async (id: string) => {
  requestIdForDialog.value = id;
  dialogActionType.value = 'approve';
  approveRejectDialog.value = true;
};

const confirmReject = async (id: string) => {
  requestIdForDialog.value = id;
  dialogActionType.value = 'reject';
  approveRejectDialog.value = true;
};

const handleApproveRejectConfirm = async (responseMessage: string | null) => {
  if (!requestIdForDialog.value || !familyId.value) return;

  const id = requestIdForDialog.value;
  let result: Result<void>;

  if (dialogActionType.value === 'approve') {
    result = await familyLinkRequestStore.approveRequest(id, familyId.value, responseMessage || undefined);
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.requests.messages.approveSuccess'), 'success');
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.requests.messages.approveError'), 'error');
    }
  } else { // reject
    result = await familyLinkRequestStore.rejectRequest(id, familyId.value, responseMessage || undefined);
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.requests.messages.rejectSuccess'), 'success');
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.requests.messages.rejectError'), 'error');
    }
  }
  loadRequests();
  approveRejectDialog.value = false;
  requestIdForDialog.value = null;
};

const handleRequestSaved = () => {
  closeAllDrawers();
  loadRequests();
};

onMounted(() => {
  loadRequests();
});

watch(familyId, (newFamilyId) => {
  if (newFamilyId) {
    loadRequests();
  }
});
</script>