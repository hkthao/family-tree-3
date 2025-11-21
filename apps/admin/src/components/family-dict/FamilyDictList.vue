<template>
  <v-data-table-server v-model:items-per-page="itemsPerPage" :headers="headers" :items="items"
    :items-length="totalItems" :loading="loading" item-value="id" @update:options="loadFamilyDicts" elevation="0"
    data-testid="family-dict-list" fixed-header>
    <template #top>
      <slot name="top">
        <v-toolbar flat>
          <v-toolbar-title>{{ t('familyDict.list.title') }}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn v-if="canPerformActions" color="primary" icon @click="$emit('import')"
            data-testid="import-family-dict-button" class="mr-2">
            <v-tooltip :text="t('familyDict.management.importFamilyDict')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-upload</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <v-btn v-if="canPerformActions" color="primary" icon @click="$emit('create')"
            data-testid="add-new-family-dict-button">
            <v-tooltip :text="t('familyDict.management.addFamilyDict')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-plus</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <v-text-field v-model="debouncedSearch" :label="t('common.search')" append-inner-icon="mdi-magnify" single-line
            hide-details clearable class="mr-2" data-test-id="family-dict-list-search-input"></v-text-field>
        </v-toolbar>
      </slot>
    </template>

    <!-- Type column -->
    <template #item.type="{ item }">
      <v-chip label size="small" class="text-capitalize">
        {{ getFamilyDictTypeTitle(item.type) }}
      </v-chip>
    </template>

    <!-- Lineage column -->
    <template #item.lineage="{ item }">
      <v-chip label size="small" class="text-capitalize">
        {{ getFamilyDictLineageTitle(item.lineage) }}
      </v-chip>
    </template>

    <!-- Names by Region column -->
    <template #item.namesByRegion="{ item }">
      <div>
        <div v-if="item.namesByRegion.north">{{ t('familyDict.form.namesByRegion.north') }}: {{ item.namesByRegion.north }}</div>
        <div v-if="item.namesByRegion.central">{{ t('familyDict.form.namesByRegion.central') }}: {{ item.namesByRegion.central }}</div>
        <div v-if="item.namesByRegion.south">{{ t('familyDict.form.namesByRegion.south') }}: {{ item.namesByRegion.south }}</div>
      </div>
    </template>

    <!-- Name column with click for details -->
    <template #item.name="{ item }">
      <div class="text-left">
        <a @click="viewFamilyDict(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer"
          aria-label="View">
          {{ item.name }}
          <div class="text-caption">{{ item.description }}</div>
        </a>
      </div>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <div v-if="canPerformActions">
        <v-menu>
          <template v-slot:activator="{ props }">
            <v-btn icon variant="text" v-bind="props" size="small">
              <v-icon>mdi-dots-vertical</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item @click="viewFamilyDict(item)" data-testid="view-family-dict-button">
              <v-list-item-title>{{ t('common.viewDetails') }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="editFamilyDict(item)" data-testid="edit-family-dict-button">
              <v-list-item-title>{{ t('common.edit') }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="confirmDelete(item)" data-testid="delete-family-dict-button">
              <v-list-item-title>{{ t('common.delete') }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </div>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" data-testid="family-dict-list-loading" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDict } from '@/types';
import type { DataTableHeader } from 'vuetify';

import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { FamilyDictType, FamilyDictLineage } from '@/types';
import { useAuth } from '@/composables/useAuth';

const props = defineProps<{
  items: FamilyDict[];
  totalItems: number;
  loading: boolean;
  search?: string;
  readOnly?: boolean;
}>();

const { isAdmin } = useAuth();

const canPerformActions = computed(() => {
  return !props.readOnly && isAdmin.value;
});

const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
  'import',
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

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const getFamilyDictTypeTitle = (type: FamilyDictType) => {
  switch (type) {
    case FamilyDictType.Blood: return t('familyDict.type.blood');
    case FamilyDictType.Marriage: return t('familyDict.type.marriage');
    case FamilyDictType.Adoption: return t('familyDict.type.adoption');
    case FamilyDictType.InLaw: return t('familyDict.type.inLaw');
    case FamilyDictType.Other: return t('familyDict.type.other');
    default: return t('common.unknown');
  }
};

const getFamilyDictLineageTitle = (lineage: FamilyDictLineage) => {
  switch (lineage) {
    case FamilyDictLineage.Noi: return t('familyDict.lineage.noi');
    case FamilyDictLineage.Ngoai: return t('familyDict.lineage.ngoai');
    case FamilyDictLineage.NoiNgoai: return t('familyDict.lineage.noiNgoai');
    case FamilyDictLineage.Other: return t('familyDict.lineage.other');
    default: return t('common.unknown');
  }
};

const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    {
      title: t('familyDict.list.headers.name'),
      key: 'name',
      width: 'auto',
      align: 'start',
    },
    {
      title: t('familyDict.list.headers.type'),
      key: 'type',
      width: '150px',
      align: 'center',
    },
    {
      title: t('familyDict.list.headers.lineage'),
      key: 'lineage',
      width: '150px',
      align: 'center',
    },
    {
      title: t('familyDict.list.headers.namesByRegion'),
      key: 'namesByRegion',
      width: '250px',
      align: 'start',
      sortable: false,
    },
  ];

  if (canPerformActions.value) {
    baseHeaders.push({
      title: t('familyDict.list.headers.actions'),
      key: 'actions',
      sortable: false,
      align: 'end',
    });
  }
  return baseHeaders;
});

const loadFamilyDicts = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  emit('update:options', options);
};

const viewFamilyDict = (familyDict: FamilyDict) => {
  emit('view', familyDict);
};

const editFamilyDict = (familyDict: FamilyDict) => {
  emit('edit', familyDict);
};

const confirmDelete = (familyDict: FamilyDict) => {
  emit('delete', familyDict);
};
</script>
