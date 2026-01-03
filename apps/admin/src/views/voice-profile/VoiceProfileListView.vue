<template>
  <div data-testid="voice-profile-list-view">
    <VoiceProfileList
      :items="voiceProfiles"
      :total-items="currentTotalItems"
      :loading="isLoadingVoiceProfiles"
      :member-id="props.memberId"
      @update:options="handleListOptionsUpdate"
      @create="openAddDrawer()"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      :allow-add="true"
      :allow-edit="true"
      :allow-delete="true"
      :is-exporting="isExporting"
      :is-importing="isImporting"
      :can-perform-actions="true"
      :on-export="exportVoiceProfiles"
      :on-import-click="() => importDialog = true"
      :search-query="filters.search"
      @update:search="setSearch"
    />

    <!-- Add Voice Profile Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleVoiceProfileClosed">
      <VoiceProfileAddView v-if="addDrawer" :member-id="props.memberId" :family-id="props.familyId" @close="handleVoiceProfileClosed"
        @saved="handleVoiceProfileSaved" />
    </BaseCrudDrawer>

    <!-- Edit Voice Profile Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleVoiceProfileClosed">
      <VoiceProfileEditView v-if="selectedItemId && editDrawer" :member-id="props.memberId" :family-id="props.familyId"
        :voice-profile-id="selectedItemId" @close="handleVoiceProfileClosed" @saved="handleVoiceProfileSaved" />
    </BaseCrudDrawer>

    <!-- Detail Voice Profile Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleVoiceProfileClosed">
      <VoiceProfileDetailView v-if="selectedItemId && detailDrawer" :member-id="props.memberId" :family-id="props.familyId"
        :voice-profile-id="selectedItemId" @close="handleVoiceProfileClosed" />
    </BaseCrudDrawer>

    <!-- Import Dialog -->
    <BaseImportDialog
      v-model="importDialog"
      :title="t('voiceProfile.import.title')"
      :label="t('voiceProfile.import.selectFile')"
      :loading="isImporting"
      :max-file-size="5 * 1024 * 1024"
      @update:model-value="importDialog = $event"
      @import="triggerImport"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useCrudDrawer, useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { useVoiceProfileDataManagement } from '@/composables/voice-profile/useVoiceProfileDataManagement';
import { useVoiceProfilesQuery } from '@/composables/voice-profile/useVoiceProfilesQuery';
import { useDeleteVoiceProfileMutation } from '@/composables/voice-profile/useDeleteVoiceProfileMutation';
import { useQueryClient } from '@tanstack/vue-query';
import type { VoiceProfile } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import VoiceProfileList from '@/components/voice-profile/VoiceProfileList.vue';
import VoiceProfileAddView from './VoiceProfileAddView.vue';
import VoiceProfileEditView from './VoiceProfileEditView.vue';
import VoiceProfileDetailView from './VoiceProfileDetailView.vue';
import BaseImportDialog from '@/components/common/BaseImportDialog.vue';
import { useVoiceProfileImportExport } from '@/composables/voice-profile/useVoiceProfileImportExport';

interface VoiceProfileListViewProps {
  memberId: string;
  familyId: string; // New prop
}

const props = defineProps<VoiceProfileListViewProps>();
const { t } = useI18n();
const queryClient = useQueryClient();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const importDialog = ref(false);

const { isExporting, isImporting, exportVoiceProfiles, importVoiceProfiles } = useVoiceProfileImportExport(computed(() => props.memberId));

const {
  state: { paginationOptions, filters },
  actions: { setPage, setItemsPerPage, setSortBy, setSearch },
} = useVoiceProfileDataManagement(computed(() => props.memberId));

const {
  state: { voiceProfiles: queryVoiceProfiles, totalItems, isLoading: isLoadingVoiceProfiles },
  actions: { refetch },
} = useVoiceProfilesQuery(
  computed(() => props.memberId),
  paginationOptions,
  filters
);

const voiceProfiles = ref<VoiceProfile[]>(queryVoiceProfiles.value || []);
const currentTotalItems = ref(totalItems.value || 0);

watch(queryVoiceProfiles, (newData) => {
  voiceProfiles.value = newData || [];
});

watch(totalItems, (newTotal) => {
  currentTotalItems.value = newTotal || 0;
});

const { mutate: deleteVoiceProfile } = useDeleteVoiceProfileMutation();

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

const triggerImport = async (file: File) => {
  if (!file) {
    showSnackbar(t('voiceProfile.messages.noFileSelected'), 'warning');
    return;
  }

  const reader = new FileReader();
  reader.onload = async (e) => {
    try {
      const jsonContent = JSON.parse(e.target?.result as string);
      await importVoiceProfiles(jsonContent);
      importDialog.value = false;
      refetch();
    } catch (error: any) {
      console.error("Import operation failed:", error);
      showSnackbar(error.message || t('voiceProfile.messages.importError'), 'error');
    }
  };
  reader.readAsText(file);
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

const confirmDelete = async (id: string) => {
  const itemToDelete = voiceProfiles.value.find(item => item.id === id);
  if (!itemToDelete) {
    showSnackbar(t('voiceProfile.messages.notFound'), 'error');
    return;
  }

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('voiceProfile.list.confirmDelete', { name: itemToDelete.label }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    handleDeleteConfirm(itemToDelete.id);
  }
};

const handleDeleteConfirm = (id: string) => {
  deleteVoiceProfile({ memberId: props.memberId, id }, {
    onSuccess: () => {
      showSnackbar(t('voiceProfile.messages.deleteSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['voice-profiles'] });
    },
    onError: (error: Error) => {
      showSnackbar(error.message || t('voiceProfile.messages.deleteError'), 'error');
    },
  });
};

const handleVoiceProfileSaved = () => {
  closeAllDrawers();
  queryClient.invalidateQueries({ queryKey: ['voice-profiles'] });
};

const handleVoiceProfileClosed = () => {
  closeAllDrawers();
};
</script>

<style scoped></style>