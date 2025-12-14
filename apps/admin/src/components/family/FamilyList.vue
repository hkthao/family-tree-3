<template>
  <v-data-table-server :items-per-page="props.itemsPerPage" @update:itemsPerPage="emit('update:itemsPerPage', $event)"
    :headers="headers" :items="props.items" :items-length="props.totalItems" :loading="props.loading" item-value="id"
    :sort-by="props.sortBy" @update:options="emit('update:options', $event)" elevation="1">
    <template #top>
      <ListToolbar
        :search="searchQuery"
        @update:search="searchQuery = $event"
        @create="emit('create')"
        :title="t('family.management.title')"
        :createButtonTooltip="t('family.list.action.create')"
        :searchPlaceholder="t('common.search')"
        createButtonTestId="add-new-family-button"
        searchInputTestId="family-list-search-input"
      />
    </template>
    <!-- Avatar column -->
    <template #item.avatarUrl="{ item }">
      <div class="d-flex justify-center">
        <v-avatar size="36" class="my-2">
          <v-img :src="getFamilyAvatarUrl(item.avatarUrl)" :alt="item.name" />
        </v-avatar>
      </div>
    </template>
    <!-- name column -->
    <template #item.name="{ item }">
      <div class="text-left">
        <a @click="$emit('view', item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer"
          aria-label="View">
          {{ item.name }}
          <div class="text-caption">{{ item.address }}</div>
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
      <v-tooltip :text="t('family.list.action.delete')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('delete', item)"
            data-testid="delete-family-button" :data-family-name="item.name" aria-label="Delete">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" data-testid="family-list-loading" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family } from '@/types';
import type { DataTableHeader } from 'vuetify';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';
import ListToolbar from '@/components/common/ListToolbar.vue';
import { useDebouncedSearch } from '@/composables/family';

const props = defineProps<{
  items: Family[];
  totalItems: number;
  loading: boolean;
  itemsPerPage: number;
  search: string;
  sortBy: { key: string; order: 'asc' | 'desc' }[];
}>();

const emit = defineEmits([
  'update:options',
  'view',
  'delete',
  'update:itemsPerPage',
  'create',
  'update:search',
  'update:sortBy',
]);

const { t } = useI18n();
const { searchQuery, debouncedSearchQuery } = useDebouncedSearch(props.search);

watch(debouncedSearchQuery, (newValue) => {
  emit('update:search', newValue);
});

watch(() => props.search, (newSearch) => {
  if (newSearch !== searchQuery.value) {
    searchQuery.value = newSearch;
  }
});

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
    sortable: true,
    width: 'auto',
    align: 'start',
  },
  {
    title: t('family.management.headers.code'),
    key: 'code',
    sortable: true,
    width: '120px',
    align: 'start',
  },
  {
    title: t('family.management.headers.totalMembers'),
    key: 'totalMembers',
    sortable: true,
    width: '120px',
    align: 'center',
  },
  {
    title: t('family.management.headers.totalGenerations'),
    key: 'totalGenerations',
    sortable: true,
    width: '120px',
    align: 'center',
  },


  {
    title: t('family.management.headers.visibility'),
    key: 'visibility',
    sortable: true,
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