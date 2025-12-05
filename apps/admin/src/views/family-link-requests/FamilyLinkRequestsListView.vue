<template>
  <div data-testid="family-link-requests-list-view">
    <!-- Removed v-card toolbar, it's now inside FamilyLinkRequestList -->
    <FamilyLinkRequestList
      :items="familyLinkRequestStore.list.items"
      :total-items="familyLinkRequestStore.list.totalItems"
      :loading="familyLinkRequestStore.list.loading"
      :read-only="readOnly"
      :search="searchQuery"
      @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer"
      @approve="confirmApprove"
      @reject="confirmReject"
    />

    <!-- Add Request Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('familyLinkRequest.form.addTitle')" icon="mdi-plus" @close="closeAddDrawer">
      <FamilyLinkRequestAddView v-if="addDrawer" :family-id="familyId"
        @close="closeAddDrawer" @saved="handleRequestSaved" />
    </BaseCrudDrawer>

    <!-- Edit Request Drawer -->
    <BaseCrudDrawer v-model="editDrawer" :title="t('familyLinkRequest.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
      <FamilyLinkRequestEditView v-if="selectedItemId && editDrawer" :family-link-request-id="selectedItemId" :family-id="familyId"
        @close="closeEditDrawer" @saved="handleRequestSaved" />
    </BaseCrudDrawer>

    <!-- Detail Request Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('familyLinkRequest.detail.title')" icon="mdi-information-outline" @close="closeDetailDrawer">
      <FamilyLinkRequestDetailView v-if="selectedItemId && detailDrawer" :family-link-request-id="selectedItemId" :family-id="familyId"
        @close="closeDetailDrawer" @edit-family-link-request="openEditDrawer" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useFamilyLinkRequestStore } from '@/stores/familyLinkRequest.store';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { storeToRefs } from 'pinia';
import { formatDate } from '@/utils/dateUtils';
import type { FamilyLinkRequestDto } from '@/types';
import { LinkStatus } from '@/types';
import type { DataTableHeader } from 'vuetify';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // New import
import { useCrudDrawer } from '@/composables/useCrudDrawer'; // New import

// New imports for the views
import { FamilyLinkRequestList } from '@/components/family-link-requests'; // NEW IMPORT
import FamilyLinkRequestAddView from '@/views/family-link-requests/FamilyLinkRequestAddView.vue';
import FamilyLinkRequestEditView from '@/views/family-link-requests/FamilyLinkRequestEditView.vue';
import FamilyLinkRequestDetailView from '@/views/family-link-requests/FamilyLinkRequestDetailView.vue';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const familyLinkRequestStore = useFamilyLinkRequestStore();
const { list } = storeToRefs(familyLinkRequestStore);
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const familyId = computed(() => route.params.familyId as string);
const searchQuery = ref(''); // NEW: Local search query state

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeEditDrawer,
  closeDetailDrawer,
  closeAllDrawers, // To close all drawers after save/delete
} = useCrudDrawer<string>(); 

const readOnly = ref(false); // Can be made reactive if needed

const headers = computed<DataTableHeader[]>(() => [
  { title: t('familyLinkRequest.list.headers.requestingFamily'), key: 'requestingFamilyName' },
  { title: t('familyLinkRequest.list.headers.targetFamily'), key: 'targetFamilyName' },
  { title: t('familyLinkRequest.list.headers.status'), key: 'status' },
  { title: t('familyLinkRequest.list.headers.requestDate'), key: 'requestDate' },
  { title: t('familyLinkRequest.list.headers.responseDate'), key: 'responseDate' },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

const getStatusColor = (status: LinkStatus) => {
  switch (status) {
    case LinkStatus.Pending: return 'warning';
    case LinkStatus.Approved: return 'success';
    case LinkStatus.Rejected: return 'error';
    default: return 'info';
  }
};

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

const confirmApprove = async (id: string) => { // NEW: Handle approve emit
  const confirmed = await showConfirmDialog({
    title: t('familyLinkRequest.list.confirmApprove.title'),
    message: t('familyLinkRequest.list.confirmApprove.message'),
    confirmText: t('familyLinkRequest.list.action.approve'),
    confirmColor: 'success',
  });

  if (confirmed && familyId.value) {
    const result = await familyLinkRequestStore.approveRequest(id, familyId.value);
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.requests.messages.approveSuccess'), 'success');
      loadRequests();
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.requests.messages.approveError'), 'error');
    }
  }
};

const confirmReject = async (id: string) => { // NEW: Handle reject emit
  const confirmed = await showConfirmDialog({
    title: t('familyLinkRequest.list.confirmReject.title'),
    message: t('familyLinkRequest.list.confirmReject.message'),
    confirmText: t('familyLinkRequest.list.action.reject'),
    confirmColor: 'error',
  });

  if (confirmed && familyId.value) {
    const result = await familyLinkRequestStore.rejectRequest(id, familyId.value);
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.requests.messages.rejectSuccess'), 'success');
      loadRequests();
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.requests.messages.rejectError'), 'error');
    }
  }
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