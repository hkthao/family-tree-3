<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useNotificationTemplateStore } from '@/stores';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { NotificationChannel, NotificationType } from '@/types';

const notificationTemplateStore = useNotificationTemplateStore();
const { items, loading, totalItems, itemsPerPage, currentPage, sortBy, filter } = storeToRefs(notificationTemplateStore);
const router = useRouter();
const { t } = useI18n();

const headers = ref([
  { title: t('notificationTemplate.list.headers.eventType'), key: 'eventType' },
  { title: t('notificationTemplate.list.headers.channel'), key: 'channel' },
  { title: t('notificationTemplate.list.headers.subject'), key: 'subject' },
  { title: t('notificationTemplate.list.headers.isActive'), key: 'isActive' },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

const searchQuery = ref('');

/**
 * Loads notification templates from the store based on current filters.
 */
const loadItems = async () => {
  notificationTemplateStore.setFilter({ search: searchQuery.value });
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

// Watcher for search query changes
watch(searchQuery, () => {
  loadItems();
});

/**
 * Navigates to the edit page for a specific notification template.
 * @param id The ID of the notification template to edit.
 */
const editTemplate = (id: string) => {
  router.push({ name: 'EditNotificationTemplate', params: { id } });
};

/**
 * Deletes a notification template after user confirmation.
 * @param id The ID of the notification template to delete.
 */
const deleteTemplate = async (id: string) => {
  if (confirm(t('notificationTemplate.list.confirmDelete'))) {
    await notificationTemplateStore.deleteItem(id);
  }
};

/**
 * Returns the translated name of a NotificationType.
 * @param type The NotificationType enum value.
 */
const getNotificationTypeName = (type: NotificationType) => {
  return t(`notificationType.${NotificationType[type]}`);
};

/**
 * Returns the translated name of a NotificationChannel.
 * @param channel The NotificationChannel enum value.
 */
const getNotificationChannelName = (channel: NotificationChannel) => {
  return t(`notificationChannel.${NotificationChannel[channel]}`);
};
</script>

<template>
  <v-card>
    <v-card-title class="d-flex align-center">
      {{ t('admin.notificationTemplates.list.title') }}
      <v-spacer></v-spacer>
      <v-text-field v-model="searchQuery" append-inner-icon="mdi-magnify" :label="t('common.search')" single-line
        hide-details density="compact" @click:append-inner="loadItems"
        @keydown.enter="loadItems"></v-text-field>
      <v-btn color="primary" class="ml-4" @click="router.push({ name: 'AddNotificationTemplate' })">
        {{ t('notificationTemplate.list.addTemplate') }}
      </v-btn>
    </v-card-title>
    <v-card-text>
      <v-data-table-server v-model:items-per-page="itemsPerPage" v-model:page="currentPage" v-model:sort-by="sortBy"
        :headers="headers" :items="items" :items-length="totalItems" :loading="loading" item-value="id"
        class="elevation-1">
        <template v-slot:item.eventType="{ item }">
          {{ getNotificationTypeName(item.eventType) }}
        </template>
        <template v-slot:item.channel="{ item }">
          {{ getNotificationChannelName(item.channel) }}
        </template>
        <template v-slot:item.isActive="{ item }">
          <v-icon :color="item.isActive ? 'success' : 'error'">
            {{ item.isActive ? 'mdi-check-circle' : 'mdi-close-circle' }}
          </v-icon>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-icon small class="me-2" @click="item.id && editTemplate(item.id)">
            mdi-pencil
          </v-icon>
          <v-icon small @click="item.id && deleteTemplate(item.id)">
            mdi-delete
          </v-icon>
        </template>
        <template v-slot:no-data>
          <v-btn color="primary" @click="loadItems">
            {{ t('common.reset') }}
          </v-btn>
        </template>
      </v-data-table-server>
    </v-card-text>
  </v-card>
</template>
