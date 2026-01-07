<template>
  <ListToolbar :title="t('userPushToken.list.title')" :create-button-tooltip="t('common.add')"
    create-button-test-id="button-create-user-push-token" @create="emit('create')" :hide-create-button="!allowAdd"
    :hide-search="false">
  </ListToolbar>
  <v-data-table-server data-testid="user-push-token-list" :headers="headers" :items="items" :items-length="totalItems"
    :loading="loading" :search="search" item-value="id" @update:options="updateOptions">
    <template v-slot:item.userId="{ item }">
      <router-link :to="{ name: 'UserProfile', params: { id: item.userId } }">
        {{ item.userId }}
      </router-link>
    </template>

    <template v-slot:item.isActive="{ item }">
      <v-icon :color="item.isActive ? 'success' : 'error'">
        {{ item.isActive ? 'mdi-check-circle' : 'mdi-cancel' }}
      </v-icon>
    </template>

    <template v-slot:item.actions="{ item }">
      <div class="d-flex ga-2">
        <v-btn icon variant="text" color="info" size="small" data-testid="button-view" @click="emit('view', item.id)">
          <v-icon>mdi-eye</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.view') }}</v-tooltip>
        </v-btn>
        <v-btn icon variant="text" color="warning" size="small" data-testid="button-edit" @click="emit('edit', item.id)"
          v-if="allowEdit">
          <v-icon>mdi-pencil</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.edit') }}</v-tooltip>
        </v-btn>
        <v-btn icon variant="text" color="error" size="small" data-testid="button-delete"
          @click="emit('delete', item.id)" v-if="allowDelete">
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
  search?: string;
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
}

const {
  items,
  totalItems,
  loading,
  search = '',
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
]);

const { t } = useI18n();

const headers = computed(() => [
  { title: t('userPushToken.list.userId'), key: 'userId' },
  { title: t('userPushToken.list.expoPushToken'), key: 'expoPushToken' },
  { title: t('userPushToken.list.platform'), key: 'platform' },
  { title: t('userPushToken.list.deviceId'), key: 'deviceId' },
  { title: t('userPushToken.list.isActive'), key: 'isActive' },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

interface DataTableOptions {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}

const updateOptions = (options: DataTableOptions) => {
  emit('update:options', options);
};
</script>

<style scoped></style>
