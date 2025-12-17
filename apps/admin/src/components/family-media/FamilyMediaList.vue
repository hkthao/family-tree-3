<template>
  <v-data-table-server
    v-model:items-per-page="itemsPerPage"
    :headers="headers"
    :items="props.items"
    :items-length="props.totalItems"
    :loading="props.loading"
    item-value="id"
    @update:options="handleOptionsUpdate"
    elevation="0"
    data-testid="family-media-list"
    fixed-header
  >
    <template #top>
      <ListToolbar
        :title="t('familyMedia.list.title')"
        :create-button-tooltip="t('familyMedia.list.createButton')"
        create-button-test-id="add-new-family-media-button"
        :hide-create-button="!allowAdd"
        :search-query="searchQuery"
        :search-label="t('common.search')"
        @update:search="searchQuery = $event"
        @create="emit('create')"
      />
    </template>
      <template v-slot:item.thumbnailPath="{ item }">
        <v-img v-if="item.thumbnailPath" :src="item.thumbnailPath" max-height="50" max-width="50" class="my-2"></v-img>
        <v-icon v-else>mdi-file-image-outline</v-icon>
      </template>
      <template v-slot:item.fileName="{ item }">
        <span class="text-primary cursor-pointer" @click="emit('view', item.id)">{{ item.fileName }}</span>
      </template>
      <template v-slot:item.mediaType="{ item }">
        {{ t(`common.mediaType.${MediaType[item.mediaType]}`) }}
      </template>
      <template v-slot:item.fileSize="{ item }">
        {{ formatBytes(item.fileSize) }}
      </template>
      <template v-slot:item.created="{ item }">
        {{ formatDate(item.created) }}
      </template>
      <template #item.actions="{ item }">
      <div v-if="allowDelete">
        <v-icon
          small
          @click="emit('delete', item.id)"
        >
          mdi-delete
        </v-icon>
      </div>
    </template>
      <template v-slot:no-data>
        <div class="text-center mt-4">{{ t('familyMedia.list.noData') }}</div>
      </template>
    </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyMedia } from '@/types';
import type { DataTableHeader } from 'vuetify';
import { MediaType } from '@/types/enums';
import { formatDate, formatBytes } from '@/utils/format.utils';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import ListToolbar from '@/components/common/ListToolbar.vue';

interface FamilyMediaListProps {
  items: FamilyMedia[];
  totalItems: number;
  loading: boolean;
  search?: string;
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
}

const props = defineProps<FamilyMediaListProps>();
const emit = defineEmits(['update:options', 'view', 'delete', 'create', 'update:search']);
const { t } = useI18n();

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const searchQuery = ref(props.search || '');
let debounceTimer: ReturnType<typeof setTimeout> | null = null;

watch(searchQuery, (newValue) => {
  if (debounceTimer) {
    clearTimeout(debounceTimer);
  }
  debounceTimer = setTimeout(() => {
    emit('update:search', newValue);
    debounceTimer = null; // Clear the timer ID after execution
  }, 300);
});

watch(() => props.search, (newSearch) => {
  if (newSearch !== searchQuery.value) {
    searchQuery.value = newSearch ?? '';
  }
});

const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    { title: '', key: 'thumbnailPath', sortable: false, width: '60px' },
    { title: t('familyMedia.list.headers.fileName'), key: 'fileName', width: 'auto' },
    { title: t('familyMedia.list.headers.mediaType'), key: 'mediaType', width: '150px' },
    { title: t('familyMedia.list.headers.fileSize'), key: 'fileSize', width: '150px' },
    { title: t('familyMedia.list.headers.created'), key: 'created', width: '150px' },
  ];

  if (props.allowAdd || props.allowEdit || props.allowDelete) {
    baseHeaders.push({
      title: t('familyMedia.list.headers.actions'),
      key: 'actions',
      sortable: false,
      align: 'end',
      minWidth: '100px',
    });
  }
  return baseHeaders;
});

const handleOptionsUpdate = (options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) => {
  emit('update:options', options);
};
</script>
