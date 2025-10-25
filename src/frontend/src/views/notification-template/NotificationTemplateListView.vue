<script setup lang="ts">
import { onMounted, watch } from 'vue';
import { useNotificationTemplateStore } from '@/stores';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { NotificationTemplateList } from '@/components/notification-template';

const notificationTemplateStore = useNotificationTemplateStore();
const { items, loading, totalItems, itemsPerPage, currentPage, sortBy } = storeToRefs(notificationTemplateStore);
const router = useRouter();
const { t } = useI18n();

/**
 * Loads notification templates from the store based on current filters.
 */
const loadItems = async () => {
  await notificationTemplateStore._loadItems();
};

onMounted(() => {
  loadItems();
});

// Watchers for pagination and sorting
watch(currentPage, () => {
  notificationTemplateStore.setPage(currentPage.value);
});

watch(itemsPerPage, () => {
  notificationTemplateStore.setItemsPerPage(itemsPerPage.value);
});

watch(sortBy, (newVal) => {
  notificationTemplateStore.setSortBy(newVal);
});

const handleSearch = (searchQuery: string) => {
  notificationTemplateStore.setFilter({ search: searchQuery });
};

const handleReset = () => {
  notificationTemplateStore.setFilter({ search: '' });
};

const handleAdd = () => {
  router.push({ name: 'AddNotificationTemplate' });
};

const handleEdit = (id: string) => {
  router.push({ name: 'EditNotificationTemplate', params: { id } });
};

const handleDelete = async (id: string) => {
  if (confirm(t('notificationTemplate.list.confirmDelete'))) {
    await notificationTemplateStore.deleteItem(id);
  }
};
</script>

<template>
  <NotificationTemplateList
    :items="items"
    :total-items="totalItems"
    :loading="loading"
    :items-per-page="itemsPerPage"
    :current-page="currentPage"
    :sort-by="sortBy"
    @update:items-per-page="notificationTemplateStore.setItemsPerPage($event)"
    @update:page="notificationTemplateStore.setPage($event)"
    @update:sort-by="notificationTemplateStore.setSortBy($event)"
    @search="handleSearch"
    @reset="handleReset"
    @add="handleAdd"
    @edit="handleEdit"
    @delete="handleDelete"
  />
</template>
