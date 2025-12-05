<template>
  <div data-testid="family-link-list-view">
    <FamilyLinkSearch @update:filters="handleFilterUpdate" />
    <FamilyLinkList
      :items="familyLinkStore.list.items"
      :total-items="familyLinkStore.list.totalItems"
      :loading="list.loading"
      @update:options="handleListOptionsUpdate"
      @unlink="handleDeleteFamilyLink"
      :read-only="props.readOnly"
    />
  </div>
</template>

<script setup lang="ts">
import { useFamilyLinkStore } from '@/stores';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { nextTick, onMounted, ref, watch } from 'vue';

import FamilyLinkSearch from '@/components/family-link/FamilyLinkSearch.vue';
import FamilyLinkList from '@/components/family-link/FamilyLinkList.vue';
import type { FamilyLinkFilter, FamilyLinkDto } from '@/types'; // Assuming these types exist

interface FamilyLinkListViewProps {
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<FamilyLinkListViewProps>();
const { t } = useI18n();
const familyLinkStore = useFamilyLinkStore();
const { list } = storeToRefs(familyLinkStore);
const searchQuery = ref('');

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const loadFamilyLinks = async () => {
  familyLinkStore.list.filters.familyId = props.familyId;
  await familyLinkStore._loadItems();
};

const handleFilterUpdate = async (filters: FamilyLinkFilter) => {
  familyLinkStore.list.filters = { ...filters, familyId: props.familyId, searchQuery: searchQuery.value };
  await familyLinkStore._loadItems();
};



const handleListOptionsUpdate = async (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  await nextTick();
  familyLinkStore.setListOptions(options);
};

const handleDeleteFamilyLink = async (link: FamilyLinkDto) => { // Renamed function
  const otherFamilyName = link.family1Id === props.familyId ? link.family2Name : link.family1Name;
  const confirmed = await showConfirmDialog({
    title: t('familyLink.links.confirmDelete.title'), // Updated translation key
    message: t('familyLink.links.confirmDelete.message', { // Updated translation key
      otherFamily: otherFamilyName,
    }),
    confirmText: t('common.delete'),
    confirmColor: 'error',
  });

  if (confirmed) {
    const result = await familyLinkStore.deleteFamilyLink(link.id); // Updated store call
    if (result.ok) {
      showSnackbar(t('familyLink.links.messages.deleteSuccess'), 'success'); // Updated translation key
      loadFamilyLinks();
    } else {
      showSnackbar(result.error?.message || t('familyLink.links.messages.deleteError'), 'error'); // Updated translation key
    }
  }
};

onMounted(() => {
  loadFamilyLinks();
});

watch(() => props.familyId, async () => {
  loadFamilyLinks();
}, { immediate: false });
</script>