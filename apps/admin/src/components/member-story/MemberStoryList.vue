<template>
  <div>
    <v-toolbar flat v-if="!hideToolbar">
      <v-toolbar-title>{{ t('memberStory.list.title') }}</v-toolbar-title>
      <v-spacer></v-spacer>
      <v-tooltip :text="t('memberStory.list.action.create')" location="bottom">
        <template v-slot:activator="{ props }">
          <v-btn color="primary" class="mr-2" v-bind="props" @click="createItem()" variant="text" icon>
            <v-icon>mdi-plus</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-text-field :model-value="search" @update:model-value="updateSearch" class="mr-2"
        append-inner-icon="mdi-magnify" :label="t('common.search')" single-line hide-details>
      </v-text-field>
    </v-toolbar>
    <v-data-table-server :items-per-page="itemsPerPage" @update:items-per-page="updateItemsPerPage" :headers="headers"
      :items="items" :items-length="totalItems" :loading="loading" @update:options="updateOptions" item-value="id"
      class="elevation-0">
      <template #item.title="{ item }">
        <a @click="viewItem(item.id)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.title }}
        </a>
      </template>
      <template #item.memberName="{ item }">
        {{ item.memberName || t('common.unknown') }}
      </template>
      <template #item.actions="{ item }">
        <v-menu>
          <template v-slot:activator="{ props: menuProps }">
            <v-btn icon variant="text" v-bind="menuProps" size="small">
              <v-icon>mdi-dots-vertical</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item @click="editItem(item.id)">
              <v-list-item-title>{{ t('common.edit') }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="deleteItem(item)">
              <v-list-item-title>{{ t('common.delete') }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </template>
    </v-data-table-server>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
import type { DataTableHeader } from 'vuetify';

interface MemberStoryListProps {
  items: MemberStoryDto[];
  totalItems: number;
  loading: boolean;
  itemsPerPage: number;
  search?: string;
  headers: DataTableHeader[];
  hideToolbar?: boolean;
}

const {
  items,
  totalItems,
  loading,
  itemsPerPage,
  search,
  headers,
  hideToolbar,
} = defineProps<MemberStoryListProps>();

const emit = defineEmits<{
  (e: 'update:options', options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }): void;
  (e: 'update:itemsPerPage', value: number): void;
  (e: 'update:search', search: string): void;
  (e: 'view', id: string): void;
  (e: 'edit', id: string): void;
  (e: 'delete', item: MemberStoryDto): void;
  (e: 'create'): void;
}>();

const { t } = useI18n();

const viewItem = (id: string | undefined) => {
  if (id) emit('view', id);
};

const editItem = (id: string | undefined) => {
  if (id) emit('edit', id);
};

const deleteItem = (item: MemberStoryDto) => {
  emit('delete', item);
};

const createItem = () => {
  emit('create');
};

const updateSearch = (search: string) => {
  emit('update:search', search);
};

const updateOptions = (options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) => {
  emit('update:options', options);
};

const updateItemsPerPage = (value: number) => {
  emit('update:itemsPerPage', value);
};
</script>