<template>
  <v-data-table-server v-model:items-per-page="itemsPerPage" :headers="headers" :items="items"
    :items-length="totalItems" :loading="loading" item-value="id" @update:options="loadMembers" elevation="0" data-testid="member-list">
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('member.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn v-if="canPerformActions" color="primary" icon @click="$emit('ai-create')">
          <v-tooltip :text="t('member.list.action.aiCreate')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-robot-happy-outline</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-btn v-if="canPerformActions" color="primary" icon @click="$emit('create')" data-testid="add-new-member-button">
          <v-tooltip :text="t('member.list.action.create')">
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
          data-test-id="member-list-search-input"
        ></v-text-field>
      </v-toolbar>
    </template>
    <!-- Avatar column -->
    <template #item.avatarUrl="{ item }">
      <div class="d-flex justify-center">
        <v-avatar size="36" class="my-2">
          <v-img v-if="item.avatarUrl" :src="item.avatarUrl" :alt="item.fullName" />
          <v-icon v-else>mdi-account-circle</v-icon>
        </v-avatar>
      </div>
    </template>

    <!-- Full Name column -->
    <template #item.fullName="{ item }">
      <a @click="viewMember(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
        {{ item.fullName }}
      </a>
    </template>

    <!-- Code column -->
    <template #item.code="{ item }">
      {{ item.code }}
    </template>

    <!-- Family column -->
    <template #item.family="{ item }">
      <ChipLookup :modelValue="item.familyId" :data-source="familyStore" display-expr="name" value-expr="id" imageExpr="avatarUrl" />
    </template>

    <!-- Date of Birth column -->
    <template #item.dateOfBirth="{ item }">
      {{ formatDate(item.dateOfBirth) }}
    </template>

    <!-- Gender column -->
    <template #item.gender="{ item }">
      <v-chip label size="small" class="text-capitalize">
        {{ getGenderTitle(item.gender) }}
      </v-chip>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <div v-if="canPerformActions">
        <v-tooltip :text="t('member.list.action.aiBiography')">
          <template v-slot:activator="{ props }">
            <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('ai-biography', item)">
              <v-icon>mdi-robot</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
        <v-tooltip :text="t('member.list.action.edit')">
          <template v-slot:activator="{ props }">
            <v-btn icon size="small" variant="text" v-bind="props" @click="editMember(item)" data-testid="edit-member-button">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
        <v-tooltip :text="t('member.list.action.delete')">
          <template v-slot:activator="{ props }">
            <v-btn icon size="small" variant="text" v-bind="props" @click="confirmDelete(item)" data-testid="delete-member-button">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
      </div>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" data-testid="member-list-loading" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import type { DataTableHeader } from 'vuetify';
import { formatDate } from '@/utils/dateUtils';
import { useFamilyStore } from '@/stores/family.store';
import { ChipLookup } from '@/components/common';
import { getGenderTitle } from '@/constants/genders'; 
import { useAuth } from '@/composables/useAuth';

const familyStore = useFamilyStore();
const { isAdmin, isFamilyManager } = useAuth();

const props = defineProps<{
  items: Member[];
  totalItems: number;
  loading: boolean;
  search: string;
  readOnly?: boolean; // Add readOnly prop
}>();

const canPerformActions = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
  'ai-biography',
  'ai-create',
  'update:search',
]);

const { t } = useI18n();

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
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

// ... (rest of the file)

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const headers = computed<DataTableHeader[]>(() => [
  {
    title: t('member.list.headers.avatar'),
    key: 'avatarUrl',
    sortable: false,
    width: '80px',
    align: 'center',
  },
  {
    title: t('member.list.headers.fullName'),
    key: 'fullName',
    width: 'auto',
    align: 'start',
  },
  {
    title: t('member.list.headers.code'),
    key: 'code',
    width: '120px',
    align: 'start',
  },
  {
    title: t('member.list.headers.family'),
    key: 'family',
    width: 'auto',
    align: 'start',
  },
  {
    title: t('member.list.headers.dateOfBirth'),
    key: 'dateOfBirth',
    width: '120px',
    align: 'center',
  },
  {
    title: t('member.list.headers.gender'),
    key: 'gender',
    width: '100px',
    align: 'center',
  },
  {
    title: t('member.list.headers.actions'),
    key: 'actions',
    sortable: false,
    width: '180px',
    align: 'center',
  },
]);

const loadMembers = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[]; // Corrected type
}) => {
  emit('update:options', options);
};

const viewMember = (member: Member) => {
  emit('view', member);
};

const editMember = (member: Member) => {
  emit('edit', member);
};

const confirmDelete = (member: Member) => {
  emit('delete', member);
};
</script>
