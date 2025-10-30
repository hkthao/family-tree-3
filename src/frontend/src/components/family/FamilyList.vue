<template>
  <v-data-table-server :items-per-page="itemsPerPage" @update:itemsPerPage="$emit('update:itemsPerPage', $event)"
    :headers="headers" :items="items" :items-length="totalItems" :loading="loading" item-value="id"
    @update:options="$emit('update:options', $event)" elevation="1">
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('family.management.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="$emit('ai-create')">
          <v-tooltip :text="t('family.list.action.aiCreate')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-robot-happy-outline</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-btn color="primary" icon @click="$emit('create')" data-testid="add-new-family-button">
          <v-tooltip :text="t('family.list.action.create')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-plus</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
      </v-toolbar>
    </template>
    <!-- Avatar column -->
    <template #item.avatarUrl="{ item }">
      <div class="d-flex justify-center">
        <v-avatar size="36" class="my-2">
          <v-img v-if="item.avatarUrl" :src="item.avatarUrl" :alt="item.name" />
          <v-icon v-else>mdi-account-group</v-icon>
        </v-avatar>
      </div>
    </template>
    <!-- name column -->
    <template #item.name="{ item }">
      <div class="text-left">
        <a @click="$emit('view', item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.name }}
        </a>
      </div>
    </template>

    <!-- code column -->
    <template #item.code="{ item }">
      {{ item.code }}
    </template>

    <!-- totalMembers column -->
    <template #item.totalMembers="{ item }">
      {{ item.totalMembers }}
    </template>

    <!-- totalGenerations column -->
    <template #item.totalGenerations="{ item }">
      {{ item.totalGenerations }}
    </template>

    <!-- visibility column -->
    <template #item.visibility="{ item }">
      <v-chip :color="item.visibility && item.visibility.toLowerCase() === 'public'
          ? 'success'
          : 'error'
        " label size="small" class="text-capitalize">
        {{
          $t(
            `family.management.visibility.${item.visibility ? item.visibility.toLowerCase() : 'private'}`,
          )
        }}
      </v-chip>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <v-tooltip :text="t('family.list.action.edit')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('edit', item)" data-testid="edit-family-button">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-tooltip :text="t('family.list.action.delete')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('delete', item)" data-testid="delete-family-button">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family } from '@/types';
import type { DataTableHeader } from 'vuetify';

const { items, totalItems, loading, itemsPerPage } =
  defineProps<{
    items: Family[];
    totalItems: number;
    loading: boolean;
    itemsPerPage: number;
  }>();

defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'update:itemsPerPage',
  'create',
  'ai-create',
]);

const { t } = useI18n();

const headers = computed<DataTableHeader[]>(() => [
  {
    title: t('family.management.headers.avatar'),
    key: 'avatarUrl',
    sortable: false,
    width: '120px',
    align: 'center',
  },
  {
    title: t('family.management.headers.name'),
    key: 'name',
    width: 'auto',
    align: 'start',
  },
  {
    title: t('family.management.headers.code'),
    key: 'code',
    width: '120px',
    align: 'start',
  },
  {
    title: t('family.management.headers.totalMembers'),
    key: 'totalMembers',
    width: '120px',
    align: 'center',
  },
  {
    title: t('family.management.headers.totalGenerations'),
    key: 'totalGenerations',
    width: '120px',
    align: 'center',
  },
  {
    title: t('family.management.headers.visibility'),
    key: 'visibility',
    width: '120px',
    align: 'center',
  },
  {
    title: t('family.management.headers.actions'),
    key: 'actions',
    sortable: false,
    width: '120px',
    align: 'center',
  },
]);
</script>
