<template>
  <div>
    <v-toolbar flat v-if="!hideToolbar">
      <v-toolbar-title>{{ t('memberStory.list.title') }}</v-toolbar-title>
      <v-spacer></v-spacer>
      <v-tooltip :text="t('memberStory.list.action.create')" location="bottom">
        <template v-slot:activator="{ props }">
          <v-btn v-if="canPerformActions" color="primary" class="mr-2" v-bind="props" @click="createItem()" variant="text" icon>
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
      <template #item.coverPhoto="{ item }">
        <v-img :src="item.memberStoryImages?.[0]?.resizedImageUrl ?? getFamilyAvatarUrl(null)" max-height="50" max-width="50" cover class="my-1 rounded"></v-img>
      </template>
      <template #item.title="{ item }">
        <a @click="viewItem(item.id)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.title }}
        </a>
      </template>
      <template #item.memberFullName="{ item }">
        <MemberName :full-name="item.memberFullName ?? undefined" :avatar-url="item.memberAvatarUrl ?? undefined" :gender="item.memberGender ?? undefined" />
      </template>
      <template #item.year="{ item }">
        {{ item.year }}
      </template>
      <template #item.lifeStage="{ item }">
        {{ item.lifeStage ? t(`lifeStage.${LifeStage[item.lifeStage]}`) : t('common.unknown') }}
      </template>
      <template #item.location="{ item }">
        {{ item.location }}
      </template>
      <template #item.actions="{ item: rowItem }">
        <div v-if="canPerformActions">
          <v-tooltip :text="t('common.edit')">
            <template v-slot:activator="{ props }">
              <v-btn icon size="small" variant="text" v-bind="props" @click="editItem(rowItem.id)"
                data-testid="edit-member-story-button" aria-label="Edit">
                <v-icon>mdi-pencil</v-icon>
              </v-btn>
            </template>
          </v-tooltip>
          <v-tooltip :text="t('common.delete')">
            <template v-slot:activator="{ props }">
              <v-btn icon size="small" variant="text" v-bind="props" @click="deleteItem(rowItem)"
                data-testid="delete-member-story-button" aria-label="Delete">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </template>
          </v-tooltip>
        </div>
      </template>
    </v-data-table-server>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
import type { DataTableHeader } from 'vuetify';
import { MemberName } from '@/components/member';
import { LifeStage } from '@/types/enums';
import { computed } from 'vue'; 
import { getFamilyAvatarUrl } from '@/utils/avatar.utils'; 
import { useAuth } from '@/composables';

interface MemberStoryListProps {
  items: MemberStoryDto[];
  totalItems: number;
  loading: boolean;
  itemsPerPage: number;
  search?: string;
  hideToolbar?: boolean;
  readOnly?: boolean; 
}

const {
  items,
  totalItems,
  loading,
  itemsPerPage,
  search,
  hideToolbar,
  readOnly, 
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
const { isAdmin, isFamilyManager } = useAuth(); 

const canPerformActions = computed(() => { 
  return !readOnly && (isAdmin.value || isFamilyManager.value);
});

const headers = computed<DataTableHeader[]>(() => [
  { title: t('memberStory.list.headers.coverPhoto'), key: 'coverPhoto', sortable: false },
  { title: t('memberStory.list.headers.title'), key: 'title' },
  { title: t('memberStory.list.headers.memberFullName'), key: 'memberFullName' },
  { title: t('memberStory.list.headers.year'), key: 'year' },
  { title: t('memberStory.list.headers.lifeStage'), key: 'lifeStage' },
  { title: t('memberStory.list.headers.location'), key: 'location' },
  { title: t('common.actions'), key: 'actions', sortable: false, align: 'end', minWidth: '120px' },
]);

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