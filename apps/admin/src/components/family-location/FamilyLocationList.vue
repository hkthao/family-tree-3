<template>
  <v-data-table-server :headers="headers" :items="props.items" :items-length="props.totalItems" :loading="props.loading"
    item-value="id" @update:options="props.readOnly ? null : emit('update:options', $event)"
    data-testid="family-location-list" class="elevation-1">
    <template #top>
      <ListToolbar :title="t('familyLocation.list.title')"
        :create-button-tooltip="t('familyLocation.list.add')"
        create-button-test-id="create-family-location-button"
        @create="emit('create')"
        :hide-create-button="!props.allowAdd"
        :hide-search="true" />
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
import { useI18n } from 'vue-i18n';
import type { FamilyLocation } from '@/types';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';
import { computed } from 'vue';
import ListToolbar from '@/components/common/ListToolbar.vue';
import type { DataTableHeader } from 'vuetify'; // Import DataTableHeader

interface FamilyLocationListProps {
  items: FamilyLocation[];
  totalItems: number;
  loading: boolean;
  readOnly?: boolean;
  familyId: string;
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
}

const props = defineProps<FamilyLocationListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create', 'update:search']);
const { t } = useI18n();


const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    { title: t('familyLocation.form.name'), key: 'name' },
    { title: t('familyLocation.form.address'), key: 'address' },
    {
      title: t('familyLocation.form.locationType'),
      key: 'locationType',
      value: (item: any) => t(`familyLocation.locationType.${LocationType[item.locationType].toLowerCase()}`),
    },
    {
      title: t('familyLocation.form.accuracy'),
      key: 'accuracy',
      value: (item: any) => t(`familyLocation.accuracy.${LocationAccuracy[item.accuracy].toLowerCase()}`),
    },
    {
      title: t('familyLocation.form.source'),
      key: 'source',
      value: (item: any) => t(`familyLocation.source.${LocationSource[item.source].toLowerCase()}`),
    },
  ];

  if (props.allowEdit || props.allowDelete) {
    baseHeaders.push({ title: t('common.actions'), key: 'actions', sortable: false });
  }

  return baseHeaders;
});
</script>
