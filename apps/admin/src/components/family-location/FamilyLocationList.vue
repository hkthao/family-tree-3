<template>
  <v-data-table-server :headers="headers" :items="props.items" :items-length="props.totalItems" :loading="props.loading"
    item-value="id" @update:options="props.readOnly ? null : emit('update:options', $event)"
    data-testid="family-location-list" class="elevation-1">
    <template #top>
      <ListToolbar :title="t('familyLocation.list.title')" :create-button-tooltip="t('familyLocation.list.add')"
        create-button-test-id="create-family-location-button" @create="emit('create')"
        :hide-create-button="!props.allowAdd" :hide-search="false">
        <template #custom-buttons>
          <!-- Export Button -->
          <v-btn color="secondary" class="mr-2" :loading="props.isExporting" :disabled="!props.canPerformActions"
            @click="props.onExport" icon>
            <v-tooltip :text="t('common.export')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-export</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <!-- Import Button -->
          <v-btn color="secondary" class="mr-2" :loading="props.isImporting" :disabled="!props.canPerformActions"
            @click="props.onImportClick" icon>
            <v-tooltip :text="t('common.import')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-import</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
        </template>
      </ListToolbar>
    </template>
    <template #item.name="{ item }">
      <span class="text-primary cursor-pointer text-decoration-underline" @click="emit('view', item.id)">
        {{ item.name }}
      </span>
    </template>
    <template #item.actions="{ item }">
      <div class="d-flex ga-2" v-if="props.allowEdit || props.allowDelete">
        <v-btn icon size="small" variant="text" color="primary" @click="emit('edit', item.id)" v-if="props.allowEdit">
          <v-icon>mdi-pencil</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.edit') }}</v-tooltip>
        </v-btn>
        <v-btn icon size="small" variant="text" color="error" @click="emit('delete', item.id)" v-if="props.allowDelete">
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
import type { FamilyLocation } from '@/types';
import ListToolbar from '@/components/common/ListToolbar.vue';
import type { DataTableHeader } from 'vuetify'; // Import DataTableHeader
import {
  getLocationTypeOptions,
  getLocationAccuracyOptions,
  getLocationSourceOptions,
} from '@/composables/utils/familyLocationOptions';

interface FamilyLocationListProps {
  items: FamilyLocation[];
  totalItems: number;
  loading: boolean;
  readOnly?: boolean;
  familyId: string;
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
  isExporting?: boolean; // New prop
  isImporting?: boolean; // New prop
  canPerformActions?: boolean; // New prop for controlling export/import button visibility
  onExport?: () => void; // New prop for export action
  onImportClick?: () => void; // New prop for import dialog click
}

const props = defineProps<FamilyLocationListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create', 'update:search']);
const { t } = useI18n();

const locationTypeMap = computed(
  () => new Map(getLocationTypeOptions(t).map((option) => [option.value, option.title])),
);
const locationAccuracyMap = computed(
  () => new Map(getLocationAccuracyOptions(t).map((option) => [option.value, option.title])),
);
const locationSourceMap = computed(
  () => new Map(getLocationSourceOptions(t).map((option) => [option.value, option.title])),
);

const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    { title: t('familyLocation.form.name'), key: 'name' },
    { title: t('familyLocation.form.address'), key: 'address' },
    {
      title: t('familyLocation.form.locationType'),
      key: 'locationType',
      value: (item: any) => locationTypeMap.value.get(item.locationType),
    },
    {
      title: t('familyLocation.form.accuracy'),
      key: 'accuracy',
      value: (item: any) => locationAccuracyMap.value.get(item.accuracy),
    },
    {
      title: t('familyLocation.form.source'),
      key: 'source',
      value: (item: any) => locationSourceMap.value.get(item.source),
    },
  ];

  if (props.allowEdit || props.allowDelete) {
    baseHeaders.push({ title: t('common.actions'), key: 'actions', sortable: false });
  }

  return baseHeaders;
});
</script>