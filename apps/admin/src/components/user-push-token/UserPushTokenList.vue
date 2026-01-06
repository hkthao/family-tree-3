<template>
  <v-container fluid data-testid="user-push-token-list">
    <v-row>
      <v-col cols="12">
        <v-card :elevation="0">
          <v-card-title class="d-flex align-center">
            <div class="text-h6">{{ t('userPushToken.list.title') }}</div>
            <v-spacer></v-spacer>
            <v-btn
              color="primary"
              data-testid="button-create-user-push-token"
              @click="emit('create')"
              v-if="allowAdd"
            >
              {{ t('common.add') }}
            </v-btn>
          </v-card-title>
          <v-card-text>
            <v-data-table-server
              :headers="headers"
              :items="items"
              :items-length="totalItems"
              :loading="loading"
              :search="search"
              item-value="id"
              @update:options="updateOptions"
            >
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
                <v-btn
                  icon
                  variant="text"
                  color="info"
                  size="small"
                  data-testid="button-view"
                  @click="emit('view', item.id)"
                >
                  <v-icon>mdi-eye</v-icon>
                </v-btn>
                <v-btn
                  icon
                  variant="text"
                  color="warning"
                  size="small"
                  data-testid="button-edit"
                  @click="emit('edit', item.id)"
                  v-if="allowEdit"
                >
                  <v-icon>mdi-pencil</v-icon>
                </v-btn>
                <v-btn
                  icon
                  variant="text"
                  color="error"
                  size="small"
                  data-testid="button-delete"
                  @click="emit('delete', item.id)"
                  v-if="allowDelete"
                >
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </template>
              <template v-slot:no-data>
                <v-alert :text="t('userPushToken.list.noData')" type="info"></v-alert>
              </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { UserPushTokenDto } from '@/types';
import { VDataTableServer } from 'vuetify/lib/components/index.mjs';

interface UserPushTokenListProps {
  items: UserPushTokenDto[];
  totalItems: number;
  loading: boolean;
  search?: string;
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
}

const props = withDefaults(defineProps<UserPushTokenListProps>(), {
  search: '',
  allowAdd: true,
  allowEdit: true,
  allowDelete: true,
});

const emit = defineEmits([
  'update:options',
  'create',
  'view',
  'edit',
  'delete',
]);

const { t } = useI18n();

const headers = computed(() => [
  { title: t('userPushToken.list.id'), key: 'id' },
  { title: t('userPushToken.list.userId'), key: 'userId' },
  { title: t('userPushToken.list.expoPushToken'), key: 'expoPushToken' },
  { title: t('userPushToken.list.platform'), key: 'platform' },
  { title: t('userPushToken.list.deviceId'), key: 'deviceId' },
  { title: t('userPushToken.list.isActive'), key: 'isActive' },
  { title: t('common.createdAt'), key: 'createdAt' },
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
