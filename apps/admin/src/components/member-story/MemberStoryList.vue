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
      <template #item.originalImageUrl="{ item }">
        <v-img :src="item.resizedImageUrl ?? item.originalImageUrl ?? getFamilyAvatarUrl(null)" max-height="50" max-width="50" cover class="my-1 rounded"></v-img>
      </template>
      <template #item.title="{ item }">
        <a @click="viewItem(item.id)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.title }}
        </a>
      </template>
      <template #item.memberFullName="{ item }">
        <MemberName :full-name="item.memberFullName ?? undefined" :avatar-url="item.memberAvatarUrl ?? undefined" :gender="item.memberGender ?? undefined" />
      </template>
      <template #item.storyStyle="{ item }">
        {{ getStoryStyleText(item.storyStyle as MemberStoryStyle) }}
      </template>
      <template #item.perspective="{ item }">
        {{ getPerspectiveText(item.perspective as MemberStoryPerspective) }}
      </template>
      <template #item.actions="{ item: rowItem }">
        <v-menu>
          <template v-slot:activator="{ props: menuProps }">
            <v-btn icon variant="text" v-bind="menuProps" size="small">
              <v-icon>mdi-dots-vertical</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item @click="() => editItem(rowItem.id)">
              <v-list-item-title>{{ t('common.edit') }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="() => deleteItem(rowItem)">
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
import { MemberName } from '@/components/member';
import { MemberStoryPerspective, MemberStoryStyle } from '@/types/enums'; // Import enums
import { computed } from 'vue'; // Import computed
import { getFamilyAvatarUrl } from '@/utils/avatar.utils'; // Import getFamilyAvatarUrl

interface MemberStoryListProps {
  items: MemberStoryDto[];
  totalItems: number;
  loading: boolean;
  itemsPerPage: number;
  search?: string;
  hideToolbar?: boolean;
}

const {
  items,
  totalItems,
  loading,
  itemsPerPage,
  search,
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

const headers = computed<DataTableHeader[]>(() => [
  { title: t('memberStory.list.headers.coverPhoto'), key: 'originalImageUrl', sortable: false },
  { title: t('memberStory.list.headers.title'), key: 'title' },
  { title: t('memberStory.list.headers.memberFullName'), key: 'memberFullName' },
  { title: t('memberStory.list.headers.storyStyle'), key: 'storyStyle' },
  { title: t('memberStory.list.headers.perspective'), key: 'perspective' },
  { title: t('common.actions'), key: 'actions', sortable: false, align: 'end' },
]);

// Helper to get display text for story style
const getStoryStyleText = (style: MemberStoryStyle | null | undefined): string => {
  switch (style) {
    case MemberStoryStyle.Nostalgic: return t('memberStory.style.nostalgic');
    case MemberStoryStyle.Warm: return t('memberStory.style.warm');
    case MemberStoryStyle.Formal: return t('memberStory.style.formal');
    case MemberStoryStyle.Folk: return t('memberStory.style.folk');
    default: return '';
  }
};

// Helper to get display text for perspective
const getPerspectiveText = (perspective: MemberStoryPerspective | null | undefined): string => {
  switch (perspective) {
    case MemberStoryPerspective.FirstPerson: return t('memberStory.create.perspective.firstPerson');
    case MemberStoryPerspective.ThirdPerson: return t('memberStory.create.perspective.thirdPerson');
    case MemberStoryPerspective.FamilyMember: return t('memberStory.create.perspective.familyMember');
    case MemberStoryPerspective.NeutralPersonal: return t('memberStory.create.perspective.neutralPersonal');
    case MemberStoryPerspective.FullyNeutral: return t('memberStory.create.perspective.fullyNeutral');
    default: return '';
  }
};

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