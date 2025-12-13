<template>
  <div>
    <v-data-table-server :headers="linkHeaders" :items="items" :items-length="totalItems" :loading="loading"
      item-value="id" class="elevation-0" @update:options="loadItems">
      <template #item.family1Name="{ item }">
        {{ item.family1Name }}
      </template>
      <template #item.family2Name="{ item }">
        {{ item.family2Name }}
      </template>
      <template #item.linkDate="{ item }">
        {{ formatDate(item.linkDate) }}
      </template>
      <template #item.actions="{ item }">
        <v-tooltip :text="t('familyLink.links.action.unlink')">
          <template v-slot:activator="{ props }">
            <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('unlink', item)">
              <v-icon>mdi-link-off</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
      </template>
    </v-data-table-server>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { type FamilyLinkDto } from '@/types';
import type { DataTableHeader } from 'vuetify';
import { formatDate } from '@/utils/dateUtils';

defineProps<{
  items: FamilyLinkDto[];
  totalItems: number;
  loading: boolean;
}>();

const emit = defineEmits([
  'update:options',
  'create',
  'unlink',
]);

const { t } = useI18n();

const linkHeaders = computed<DataTableHeader[]>(() => [
  { title: t('familyLink.links.headers.family1'), key: 'family1Name', sortable: false },
  { title: t('familyLink.links.headers.family2'), key: 'family2Name', sortable: false },
  { title: t('familyLink.links.headers.linkDate'), key: 'linkDate' },
  { title: t('common.actions'), key: 'actions', sortable: false, width: '120px', align: 'center' },
]);

const loadItems = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  emit('update:options', options);
};
</script>