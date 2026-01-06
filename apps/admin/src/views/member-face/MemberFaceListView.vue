<template>
  <div data-testid="member-face-list-view">
    <div v-if="hasConsent">
      <MemberFaceSearch @update:filters="handleFilterUpdate" />

      <MemberFaceList :items="memberFaces" :total-items="totalItems" :loading="queryLoading || isDeleting"
        :search="searchQuery" :items-per-page="itemsPerPage" :sortBy="sortBy" @update:options="handleListOptionsUpdate"
        @view="openDetailDrawer" @delete="confirmDelete" @create="openAddDrawer()" @update:search="handleSearchUpdate"
        :is-exporting="isExporting" :is-importing="isImporting" :can-perform-actions="true" :on-export="exportMemberFaces"
        :on-import-click="() => importDialog = true" />

      <!-- Import Dialog -->
      <BaseImportDialog v-model="importDialog" :title="t('memberFace.import.title')"
        :label="t('memberFace.import.selectFile')" :loading="isImporting" :max-file-size="5 * 1024 * 1024"
        @update:model-value="importDialog = $event" @import="triggerImport" />

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
    <div v-else class="text-center pa-5">
      <FaceDataConsentCard
        :handle-grant-consent="handleGrantConsent"
        :handle-deny-consent="handleDenyConsent"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed, ref, toRef, watch } from 'vue';
import { useCrudDrawer } from '@/composables';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import type { MemberFace, MemberFaceFilter, ListOptions, FilterOptions } from '@/types';
import MemberFaceList from '@/components/member-face/MemberFaceList.vue';
import MemberFaceAddView from '@/views/member-face/MemberFaceAddView.vue';
import MemberFaceDetailView from '@/views/member-face/MemberFaceDetailView.vue';
import MemberFaceSearch from '@/components/member-face/MemberFaceSearch.vue';
import { useMemberFaceListFilters, useMemberFacesQuery, useDeleteMemberFaceMutation, useMemberFaceDeletion } from '@/composables';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import BaseImportDialog from '@/components/common/BaseImportDialog.vue';
import { useMemberFaceImportExport } from '@/composables/member-face/useMemberFaceImportExport';
import { useFaceDataConsent } from '@/composables/useFaceDataConsent';
import FaceDataConsentCard from '@/components/common/FaceDataConsentCard.vue';

interface MemberFaceListViewProps {
  memberId?: string;
  familyId?: string;
}
const props = defineProps<MemberFaceListViewProps>();
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { checkConsent, setConsent, denyConsent } = useFaceDataConsent();

const hasConsent = ref(false);

const {
  addDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const importDialog = ref(false);

const memberFaceListFiltersComposables = useMemberFaceListFilters();
const {
  state: { searchQuery, page, itemsPerPage, sortBy, filters },
  actions: { setPage, setItemsPerPage, setSortBy, setSearchQuery, setFilters },
} = memberFaceListFiltersComposables;

const { isExporting, isImporting, exportMemberFaces, importMemberFaces } = useMemberFaceImportExport(toRef(props, 'memberId'), toRef(props, 'familyId'));

const triggerImport = async (file: File) => {
  if (!file) {
    showSnackbar(t('memberFace.messages.noFileSelected'), 'warning');
    return;
  }

  const reader = new FileReader();
  reader.onload = async (e) => {
    try {
      const jsonContent = JSON.parse(e.target?.result as string);
      await importMemberFaces(jsonContent);
      importDialog.value = false;
      refetch();
    } catch (error: any) {
      console.error("Import operation failed:", error);
    }
  };
  reader.readAsText(file);
};

  const listOptions = computed<ListOptions>(() => ({
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value.map((s: { key: string; order: 'asc' | 'desc' }) => ({ key: s.key, order: s.order as 'asc' | 'desc' })),
  }));
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
  refetch();
};

const handleMemberFaceClosed = () => {
  closeAllDrawers();
};

const handleDetailClosed = () => {
  closeAllDrawers();
};

const handleGrantConsent = () => {
  setConsent();
  hasConsent.value = true;
  showSnackbar(t('faceDataConsent.grantSuccess'), 'success');
  refetch();
};

const handleDenyConsent = () => {
  denyConsent();
  hasConsent.value = false;
  showSnackbar(t('faceDataConsent.denySuccess'), 'info');
};

onMounted(() => {
  setFilters({
    memberId: props.memberId,
    familyId: props.familyId,
  });
  hasConsent.value = checkConsent();
});

watch([() => props.memberId, () => props.familyId], ([newMemberId, newFamilyId]) => {
  setFilters({
    memberId: newMemberId,
    familyId: newFamilyId,
  });
});
</script>