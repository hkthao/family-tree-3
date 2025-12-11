<template>
  <div data-testid="family-media-list-view">
    <FamilyMediaSearch @update:filters="handleFilterUpdate" />
    <FamilyMediaList
      :items="familyMediaStore.list.items"
      :total-items="familyMediaStore.list.totalItems"
      :loading="familyMediaStore.list.loading"
      @update:options="handleListOptionsUpdate"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer()"
    />

    <!-- Add/Edit/Detail Drawers (similar to MemberListView) -->
    <!-- Detail Family Media Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <FamilyMediaDetailView v-if="selectedItemId && detailDrawer" :family-media-id="selectedItemId" @close="handleDetailClosed" />
    </BaseCrudDrawer>

    <!-- Edit Family Media Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleMediaClosed">
      <FamilyMediaEditView v-if="selectedItemId && editDrawer" :family-media-id="selectedItemId" @close="handleMediaClosed" @saved="handleMediaSaved" />
    </BaseCrudDrawer>

    <!-- Add Family Media Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMediaClosed">
      <FamilyMediaAddView v-if="addDrawer" :family-id="currentFamilyId" @close="handleMediaClosed" @saved="handleMediaSaved" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { useFamilyMediaStore } from '@/stores/familyMedia.store';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import type { FamilyMediaFilter, ListOptions, FamilyMedia } from '@/types';

// Components
import FamilyMediaSearch from '@/components/family-media/FamilyMediaSearch.vue';
import FamilyMediaList from '@/components/family-media/FamilyMediaList.vue';
import FamilyMediaAddView from '@/views/family-media/FamilyMediaAddView.vue'; // Will create these views later
import FamilyMediaEditView from '@/views/family-media/FamilyMediaEditView.vue'; // Will create these views later
import FamilyMediaDetailView from '@/views/family-media/FamilyMediaDetailView.vue'; // Will create these views later
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // Assuming this exists

const { t } = useI18n();
const familyMediaStore = useFamilyMediaStore();
const { list } = storeToRefs(familyMediaStore);
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

// Assuming there's a way to get the current family ID, perhaps from a family store or route param
// For now, let's hardcode or get from a placeholder
const currentFamilyId = ref('YOUR_FAMILY_ID_HERE'); // TODO: Replace with actual family ID from context/store

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

const handleFilterUpdate = async (filters: FamilyMediaFilter) => {
  familyMediaStore.list.filters = { ...filters, familyId: currentFamilyId.value };
  await familyMediaStore._loadItems();
};

const handleListOptionsUpdate = async (options: ListOptions) => {
  await nextTick();
  familyMediaStore.setListOptions(options);
};

const confirmDelete = async (familyMediaId: string) => {
  const media = familyMediaStore.list.items.find(m => m.id === familyMediaId);
  if (!media) {
    showSnackbar(t('familyMedia.messages.notFound'), 'error');
    return;
  }
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyMedia.list.confirmDelete', { fileName: media.fileName || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });
  if (confirmed) {
    await handleDeleteConfirm(media);
  }
};

const handleDeleteConfirm = async (media: FamilyMedia) => {
  if (media) {
    await familyMediaStore.deleteFamilyMedia(media.familyId, media.id);
    if (familyMediaStore.list.error) {
      showSnackbar(t('familyMedia.messages.deleteError', { error: familyMediaStore.list.error }), 'error');
    } else {
      showSnackbar(t('familyMedia.messages.deleteSuccess'), 'success');
    }
  }
  familyMediaStore._loadItems();
};

const handleMediaSaved = () => {
  closeAllDrawers();
  familyMediaStore._loadItems();
};

const handleMediaClosed = () => {
  closeAllDrawers();
};

const handleDetailClosed = () => {
  closeAllDrawers();
};

onMounted(() => {
  if (currentFamilyId.value) {
    familyMediaStore.list.filters = { familyId: currentFamilyId.value };
    familyMediaStore._loadItems();
  } else {
    showSnackbar(t('familyMedia.errors.familyIdRequired'), 'error');
  }
});
</script>

<style scoped>
/* Scoped styles for the view */
</style>
