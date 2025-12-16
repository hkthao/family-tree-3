<template>
  <v-data-table-server
    :headers="headers"
    :items="props.items"
    :items-length="props.totalItems"
    :loading="props.loading"
    :search="props.search"
    item-value="id"
    @update:options="props.readOnly ? null : emit('update:options', $event)"
    data-testid="family-location-list"
    class="elevation-1"
  >
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('familyLocation.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn
          color="primary"
          dark
          class="mb-2"
          @click="emit('create')"
          data-testid="create-family-location-button"
          v-if="!props.readOnly"
        >
          {{ t('familyLocation.list.add') }}
        </v-btn>
      </v-toolbar>
    </template>
    <template #item.actions="{ item }">
      <div class="d-flex ga-2">
        <v-btn icon size="small" variant="tonal" color="info" @click="emit('view', item.id)">
          <v-icon>mdi-eye</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.view') }}</v-tooltip>
        </v-btn>
        <v-btn
          icon
          size="small"
          variant="tonal"
          color="primary"
          @click="emit('edit', item.id)"
          v-if="!props.readOnly"
        >
          <v-icon>mdi-pencil</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.edit') }}</v-tooltip>
        </v-btn>
        <v-btn
          icon
          size="small"
          variant="tonal"
          color="error"
          @click="emit('delete', item.id)"
          v-if="!props.readOnly"
        >
          <v-icon>mdi-delete</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.delete') }}</v-tooltip>
        </v-btn>
      </div>
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { FamilyLocation } from '@/types';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';
import { computed } from 'vue';

interface FamilyLocationListProps {
  items: FamilyLocation[];
  totalItems: number;
  loading: boolean;
  search: string;
  readOnly?: boolean;
}

const props = defineProps<FamilyLocationListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create']);
const { t } = useI18n();

const headers = computed(() => [
  { title: t('familyLocation.form.name'), key: 'name' },
  { title: t('familyLocation.form.address'), key: 'address' },
  {
    title: t('familyLocation.form.locationType'),
    key: 'locationType',
    value: (item: FamilyLocation) => t(`familyLocation.locationType.${LocationType[item.locationType].toLowerCase()}`),
  },
  {
    title: t('familyLocation.form.accuracy'),
    key: 'accuracy',
    value: (item: FamilyLocation) => t(`familyLocation.accuracy.${LocationAccuracy[item.accuracy].toLowerCase()}`),
  },
  {
    title: t('familyLocation.form.source'),
    key: 'source',
    value: (item: FamilyLocation) => t(`familyLocation.source.${LocationSource[item.source].toLowerCase()}`),
  },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);
</script>
