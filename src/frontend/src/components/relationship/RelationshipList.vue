<template>
  <v-card data-testid="relationship-list">
    <v-data-table :headers="headers" :items="formattedRelationships" :loading="relationshipStore.list.loading"
      :items-per-page="relationshipStore.list.itemsPerPage" :total-items="relationshipStore.list.totalItems"
      @update:options="loadItems" elevation="0">
      <template #top>
        <v-toolbar flat>
          <v-toolbar-title>{{ t('relationship.list.title') }}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn color="primary" icon @click="showAiCreatePopup = true" data-testid="relationship-ai-create-button">
            <v-tooltip :text="t('relationship.list.action.aiCreate')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-robot-happy-outline</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <v-btn color="primary" icon @click="$emit('create')" data-testid="relationship-create-button">
            <v-tooltip :text="t('relationship.list.action.create')">
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
            data-test-id="relationship-list-search-input"
          ></v-text-field>
        </v-toolbar>
      </template>
      <template v-slot:item.formattedRelationship="{ item }">
        <span v-html="item.formattedRelationship" @click="handleRelationshipClick($event, item)"></span>
      </template>
      <template v-slot:item.actions="{ item }">
        <v-tooltip :text="t('relationship.list.action.edit')">
          <template v-slot:activator="{ props }">
            <v-icon small class="mr-2" v-bind="props" @click="editItem(item)" data-testid="relationship-edit-button">mdi-pencil</v-icon>
          </template>
        </v-tooltip>
        <v-tooltip :text="t('relationship.list.action.delete')">
          <template v-slot:activator="{ props }">
            <v-icon small v-bind="props" @click="deleteItem(item)" data-testid="relationship-delete-button">mdi-delete</v-icon>
          </template>
        </v-tooltip>
      </template>
    </v-data-table>
  </v-card>
  <NLRelationshipPopup v-model="showAiCreatePopup" @saved="relationshipStore._loadItems()" />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Relationship } from '@/types';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';
import NLRelationshipPopup from './NLRelationshipPopup.vue';

const props = defineProps<{
  search: string;
}>();

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const showAiCreatePopup = ref(false);

const headers = computed(() => [
  { title: t('relationship.list.headers.relationship'), key: 'formattedRelationship', sortable: true, align: 'start' as const },
  {
    title: t('common.actions'), key: 'actions', sortable: false, width: '120px',
    align: 'center' as const,
  },
]);

const formattedRelationships = computed(() => {
  return relationshipStore.list.items.map(item => ({
    ...item,
    formattedRelationship: `
      <a class="text-primary font-weight-bold text-decoration-underline cursor-pointer" data-member-id="${item.sourceMemberId}">${item.sourceMember?.fullName}</a>
      <a class="text-green font-weight-bold text-decoration-underline cursor-pointer">${t('relationship.isThe')} ${getRelationshipTypeTitle(item.type)} ${t('relationship.of')}</a>
      <a class="text-primary font-weight-bold text-decoration-underline cursor-pointer" data-member-id="${item.targetMemberId}">${item.targetMember?.fullName}</a>
    `,
  }));
});


const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
  'ai-create',
  'view-member',
  'update:search',
]);

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

const loadItems = async (options: { page: number; itemsPerPage: number; sortBy: any[] }) => {
  emit('update:options', options);
};


const editItem = (item: Relationship) => {
  emit('edit', item);
};

const deleteItem = (item: Relationship) => {
  emit('delete', item);
};

const handleRelationshipClick = (event: MouseEvent, item: Relationship) => {
  const target = event.target as HTMLElement;
  if (target.tagName === 'A' && target.dataset.memberId) {
    emit('view-member', target.dataset.memberId);
  } else {
    // If the click is not on a member link, view the relationship itself
    emit('view', item);
  }
};
</script>
