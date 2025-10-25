<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { NotificationChannel, NotificationType } from '@/types';
import type { NotificationTemplate } from '@/types';
import type { DataTableHeader } from 'vuetify';

const props = defineProps<{
  items: NotificationTemplate[];
  totalItems: number;
  loading: boolean;
  itemsPerPage: number;
  currentPage: number;
  sortBy: { key: string; order?: 'asc' | 'desc' }[];
}>();

const emit = defineEmits([
  'update:itemsPerPage',
  'update:page',
  'update:sortBy',
  'edit',
  'delete',
  'add',
]);

const { t } = useI18n();

const headers = computed<DataTableHeader[]>(() => [
  { title: t('notificationTemplate.list.headers.eventType'), key: 'eventType' },
  { title: t('notificationTemplate.list.headers.channel'), key: 'channel' },
  { title: t('notificationTemplate.list.headers.subject'), key: 'subject' },
  { title: t('notificationTemplate.list.headers.isActive'), key: 'isActive' },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

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
  <v-data-table-server
    :items-per-page="props.itemsPerPage"
    @update:items-per-page="emit('update:itemsPerPage', $event)"
    :page="props.currentPage"
    @update:page="emit('update:page', $event)"
    :sort-by="props.sortBy"
    @update:sort-by="emit('update:sortBy', $event)"
    :headers="headers"
    :items="props.items"
    :items-length="props.totalItems"
    :loading="props.loading"
    item-value="id"
    elevation="1"
  >
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('admin.notificationTemplates.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="emit('add')">
          <v-tooltip :text="t('notificationTemplate.list.addTemplate')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-plus</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
      </v-toolbar>
    </template>

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
      <v-icon small class="me-2" @click="item.id && emit('edit', item.id)">
        mdi-pencil
      </v-icon>
      <v-icon small @click="item.id && emit('delete', item.id)">
        mdi-delete
      </v-icon>
    </template>
    <template v-slot:no-data>
      <v-btn color="primary" @click="emit('reset')">
        {{ t('common.reset') }}
      </v-btn>
    </template>
  </v-data-table-server>
</template>
