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
      <slot name="top">
        <v-toolbar flat>
          <v-toolbar-title>{{ t('familyMedia.list.title') }}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn
            v-if="canPerformActions"
            color="primary"
            icon
            @click="emit('create')"
            data-testid="add-new-family-media-button"
          >
            <v-tooltip :text="t('familyMedia.list.createButton')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-plus</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <v-text-field
            v-model="debouncedSearch"
            :label="t('common.search')"
            append-inner-icon="mdi-magnify"
            single-line
            hide-details
            clearable
            class="mr-2"
            data-test-id="family-media-list-search-input"
          ></v-text-field>
        </v-toolbar>
      </slot>
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
      <div v-if="canPerformActions">
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
import { useAuth } from '@/composables';

interface FamilyMediaListProps {
  items: FamilyMedia[];
  totalItems: number;
  loading: boolean;
  search?: string;
  readOnly?: boolean;
}

const props = defineProps<FamilyMediaListProps>();
const emit = defineEmits(['update:options', 'view', 'delete', 'create', 'update:search']);
const { t } = useI18n();
const { isAdmin } = useAuth();

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const canPerformActions = computed(() => {
  return !props.readOnly && isAdmin.value;
});

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
    { title: '', key: 'thumbnailPath', sortable: false, width: '60px' },
    { title: t('familyMedia.list.headers.fileName'), key: 'fileName', width: 'auto' },
    { title: t('familyMedia.list.headers.mediaType'), key: 'mediaType', width: '150px' },
    { title: t('familyMedia.list.headers.fileSize'), key: 'fileSize', width: '150px' },
    { title: t('familyMedia.list.headers.created'), key: 'created', width: '150px' },
  ];

  if (canPerformActions.value) {
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
