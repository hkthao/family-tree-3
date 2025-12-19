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
      <div class="d-flex flex-row flex-wrap ga-1 my-1">
        <v-tooltip v-if="item.namesByRegion.north" :text="t('familyDict.form.namesByRegion.north')">
          <template v-slot:activator="{ props: tooltipProps }">
            <v-chip v-bind="tooltipProps" size="small" color="blue" prepend-icon="mdi-compass-outline">
              {{ item.namesByRegion.north }}
            </v-chip>
          </template>
        </v-tooltip>
        <template v-if="item.namesByRegion.central">
          <v-tooltip v-if="typeof item.namesByRegion.central === 'string'" :text="t('familyDict.form.namesByRegion.central')">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-chip v-bind="tooltipProps" size="small" color="green" prepend-icon="mdi-map-marker-outline">
                {{ item.namesByRegion.central }}
              </v-chip>
            </template>
          </v-tooltip>
          <v-tooltip v-else v-for="(name, i) in item.namesByRegion.central" :key="`central-${i}`" :text="t('familyDict.form.namesByRegion.central')">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-chip v-bind="tooltipProps" size="small" color="green" prepend-icon="mdi-map-marker-outline">
                {{ name }}
              </v-chip>
            </template>
          </v-tooltip>
        </template>
        <template v-if="item.namesByRegion.south">
          <v-tooltip v-if="typeof item.namesByRegion.south === 'string'" :text="t('familyDict.form.namesByRegion.south')">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-chip v-bind="tooltipProps" size="small" color="red" prepend-icon="mdi-compass">
                 {{ item.namesByRegion.south }}
              </v-chip>
            </template>
          </v-tooltip>
          <v-tooltip v-else v-for="(name, i) in item.namesByRegion.south" :key="`south-${i}`" :text="t('familyDict.form.namesByRegion.south')">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-chip v-bind="tooltipProps" size="small" color="red" prepend-icon="mdi-compass">
                {{ name }}
              </v-chip>
            </template>
          </v-tooltip>
        </template>
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
        <v-btn icon variant="text" size="small" @click="editFamilyDict(item)" data-testid="edit-family-dict-button">
          <v-tooltip :text="t('common.edit')" activator="parent" location="top" />
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        <v-btn icon variant="text" size="small" @click="confirmDelete(item)" data-testid="delete-family-dict-button">
          <v-tooltip :text="t('common.delete')" activator="parent" location="top" />
          <v-icon>mdi-delete</v-icon>
        </v-btn>
      </div>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" data-testid="family-dict-list-loading" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { FamilyDict } from '@/types';
import { useFamilyDictList } from '@/composables';

const props = defineProps<{
  items: FamilyDict[];
  totalItems: number;
  loading: boolean;
  search?: string;
  readOnly?: boolean;
}>();

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

const {
  debouncedSearch,
  itemsPerPage,
  headers,
  canPerformActions,
  getFamilyDictTypeTitle,
  getFamilyDictLineageTitle,
  loadFamilyDicts,
  viewFamilyDict,
  editFamilyDict,
  confirmDelete,
} = useFamilyDictList(props, emit);
</script>
