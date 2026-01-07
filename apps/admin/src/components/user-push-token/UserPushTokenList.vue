<template>
  <v-data-table-server data-testid="user-push-token-list" :headers="headers" :items="items" :items-length="totalItems"
    :loading="loading" item-value="id" @update:options="updateOptions">
    <template #top>
      <ListToolbar :title="t('userPushToken.list.title')" :create-button-tooltip="t('common.add')"
        create-button-test-id="button-create-user-push-token" @create="emit('create')" :hide-create-button="!allowAdd"
        :hide-search="false" :search-query="searchQuery" @update:search="emit('update:search', $event)">
      </ListToolbar>
    </template>
    <template v-slot:item.userId="{ item }">
      {{ item.userId }}
    </template>

    <template v-slot:item.expoPushToken="{ item }">
      <div class="d-flex align-center">
        <a href="#" @click="emit('view', item.id)">
          {{ item.expoPushToken }}
          <v-tooltip activator="parent" location="top">{{ t('common.view') }}</v-tooltip>
        </a>
        <v-icon size="small" class="ml-2" @click.stop="copyToClipboard(item.expoPushToken)">
          mdi-content-copy
          <v-tooltip activator="parent" location="top">{{ t('common.copy') }}</v-tooltip>
        </v-icon>
      </div>
    </template>

    <template v-slot:item.isActive="{ item }">
      <v-icon :color="item.isActive ? 'success' : 'error'">
        {{ item.isActive ? 'mdi-check-circle' : 'mdi-cancel' }}
      </v-icon>
    </template>

    <template v-slot:item.actions="{ item }">
      <div class="d-flex ga-2">
        <v-btn icon variant="text" size="small" data-testid="button-edit" @click="emit('edit', item.id)"
          v-if="allowEdit">
          <v-icon>mdi-pencil</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.edit') }}</v-tooltip>
        </v-btn>
        <v-btn icon variant="text" size="small" data-testid="button-delete" @click="emit('delete', item.id)"
          v-if="allowDelete">
          <v-icon>mdi-delete</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.delete') }}</v-tooltip>
        </v-btn>
      </div>
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { UserPushTokenDto } from '@/types';
import { VDataTableServer } from 'vuetify/lib/components/index.mjs';
import ListToolbar from '@/components/common/ListToolbar.vue';

interface UserPushTokenListProps {
  items: UserPushTokenDto[];
  totalItems: number;
  loading: boolean;
  searchQuery?: string; // Add this prop
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
}

const {
  items,
  totalItems,
  loading,
  searchQuery = '', // Initialize with empty string
  allowAdd = true,
  allowEdit = true,
  allowDelete = true,
} = defineProps<UserPushTokenListProps>();

const emit = defineEmits([
  'update:options',
  'create',
  'view',
  'edit',
  'delete',
  'update:search', // Add this emit
]);

const { t } = useI18n();

const headers = computed(() => [
  { title: t('userPushToken.list.userId'), key: 'userId' },
  { title: t('userPushToken.list.expoPushToken'), key: 'expoPushToken' },
  { title: t('userPushToken.list.platform'), key: 'platform', align: 'center', minWidth: 120 },
  { title: t('userPushToken.list.deviceId'), key: 'deviceId' },
  { title: t('userPushToken.list.isActive'), key: 'isActive', align: 'center', minWidth: 120  },
  { title: t('common.actions'), key: 'actions', sortable: false, align: 'center' },
]);

interface DataTableOptions {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}

const updateOptions = (options: DataTableOptions) => {
  emit('update:options', options);
};

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text);
    // Optionally, add a notification for success
    console.log('Copied to clipboard:', text);
  } catch (err) {
    // Optionally, add a notification for failure
    console.error('Failed to copy to clipboard:', err);
  }
};
</script>

<style scoped></style>