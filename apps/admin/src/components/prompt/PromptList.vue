<template>
  <v-data-table-server :headers="headers" :items="items"
    :items-length="totalItems" :loading="loading" item-value="id" @update:options="loadPrompts" elevation="0"
    data-testid="prompt-list" fixed-header :page="props.page" :items-per-page="props.itemsPerPage">
    <template #top>
      <slot name="top">
        <v-toolbar flat>
          <v-toolbar-title>{{ t('prompt.list.title') }}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn v-if="canPerformActions" color="primary" icon @click="$emit('create')"
            data-testid="add-new-prompt-button">
            <v-tooltip :text="t('prompt.management.addPrompt')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-plus</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <v-text-field v-model="debouncedSearch" :label="t('common.search')" append-inner-icon="mdi-magnify" single-line
            hide-details clearable class="mr-2" data-test-id="prompt-list-search-input"></v-text-field>
        </v-toolbar>
      </slot>
    </template>

    <!-- Code column -->
    <template #item.code="{ item }">
      <div class="text-left">
        <span class="font-weight-bold">{{ item.code }}</span>
      </div>
    </template>

    <!-- Title column with click for details -->
    <template #item.title="{ item }">
      <div class="text-left">
        <a @click="viewPrompt(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer"
          aria-label="View">
          {{ item.title }}
        </a>
      </div>
    </template>

    <!-- Content column -->
    <template #item.content="{ item }">
      <div class="text-left">
        {{ item.content }}
      </div>
    </template>

    <!-- Description column -->
    <template #item.description="{ item }">
      <div class="text-left">
        {{ item.description }}
      </div>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <div v-if="canPerformActions">
        <v-tooltip :text="t('common.edit')">
          <template v-slot:activator="{ props }">
            <v-btn icon size="small" variant="text" v-bind="props" @click="editPrompt(item)"
              data-testid="edit-prompt-button" aria-label="Edit">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
        <v-tooltip :text="t('common.delete')">
          <template v-slot:activator="{ props }">
            <v-btn icon size="small" variant="text" v-bind="props" @click="confirmDeletePrompt(item)"
              data-testid="delete-prompt-button" :data-prompt-code="item.code" aria-label="Delete">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
      </div>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" data-testid="prompt-list-loading" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Prompt } from '@/types/prompt';
import type { DataTableHeader } from 'vuetify';

import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { useAuth } from '@/composables';

const props = defineProps<{
  items: Prompt[];
  totalItems: number;
  loading: boolean;
  search?: string;
  page?: number; // Added
  itemsPerPage?: number; // Added
  readOnly?: boolean;
}>();

const { state } = useAuth();

const canPerformActions = computed(() => {
  return !props.readOnly && state.isAdmin.value;
});

const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
  'update:search',
]);

const { t } = useI18n();

const searchQuery = ref(props.search);
let debounceTimer: ReturnType<typeof setTimeout>;

const debouncedSearch = computed({
  get() {
    return searchQuery.value;
  },
  set(newValue: string) {
    searchQuery.value = newValue;
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      emit('update:search', newValue);
    }, 300);
  },
});

watch(() => props.search, (newSearch) => {
  if (newSearch !== searchQuery.value) {
    searchQuery.value = newSearch;
  }
});



const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    {
      title: t('prompt.list.headers.code'),
      key: 'code',
      width: '100px',
      align: 'start',
    },
    {
      title: t('prompt.list.headers.title'),
      key: 'title',
      width: 'auto',
      align: 'start',
    },
    {
      title: t('prompt.list.headers.description'),
      key: 'description',
      width: 'auto',
      align: 'start',
      sortable: false, // Description can be long, not typically sorted by
    },
  ];

  if (canPerformActions.value) {
    baseHeaders.push({
      title: t('prompt.list.headers.actions'),
      key: 'actions',
      sortable: false,
      align: 'end',
      width: '150px',
    });
  }
  return baseHeaders;
});

const loadPrompts = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  emit('update:options', options);
};

const viewPrompt = (prompt: Prompt) => {
  emit('view', prompt.id);
};

const editPrompt = (prompt: Prompt) => {
  emit('edit', prompt.id);
};

const confirmDeletePrompt = (prompt: Prompt) => {
  emit('delete', prompt);
};
</script>
