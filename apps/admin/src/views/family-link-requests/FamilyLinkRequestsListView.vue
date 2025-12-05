<template>
  <div data-testid="family-link-requests-list-view">
    <v-card :elevation="0" class="mb-4">
      <v-card-title class="text-h6 d-flex align-center">
        {{ t('familyLinkRequest.list.title') }}
        <v-spacer></v-spacer>
        <v-btn
          color="primary"
          @click="openAddDrawer()"
          :disabled="!familyId"
        >
          {{ t('familyLinkRequest.list.action.create') }}
        </v-btn>
      </v-card-title>
    </v-card>

    <v-data-table-server
      :headers="headers"
      :items="familyLinkRequestStore.list.items"
      :items-length="familyLinkRequestStore.list.totalItems"
      :loading="familyLinkRequestStore.list.loading"
      item-value="id"
      class="elevation-0"
      @update:options="handleListOptionsUpdate"
    >
      <template #item.requestingFamilyName="{ item }">
        {{ item.requestingFamilyName }}
      </template>
      <template #item.targetFamilyName="{ item }">
        {{ item.targetFamilyName }}
      </template>
      <template #item.status="{ item }">
        <v-chip :color="getStatusColor(item.status)">
          {{ t(`familyLinkRequest.status.${item.status.toLowerCase()}`) }}
        </v-chip>
      </template>
      <template #item.requestDate="{ item }">
        {{ formatDate(item.requestDate) }}
      </template>
      <template #item.responseDate="{ item }">
        {{ item.responseDate ? formatDate(item.responseDate) : t('common.na') }}
      </template>
      <template #item.actions="{ item }">
        <v-btn
          icon
          size="small"
          variant="text"
          @click="openDetailDrawer(item.id)"
        >
          <v-icon>mdi-eye</v-icon>
        </v-btn>
        <v-btn
          icon
          size="small"
          variant="text"
          @click="openEditDrawer(item.id)"
        >
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        <v-btn
          icon
          size="small"
          variant="text"
          @click="confirmDelete(item.id)"
        >
          <v-icon>mdi-delete</v-icon>
        </v-btn>
      </template>
    </v-data-table-server>

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

const handleRequestSaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  loadRequests(); // Reload list after save
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