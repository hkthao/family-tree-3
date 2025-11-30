<template>
  <v-card :elevation="0">
    <v-card-text>
      <v-data-table-server v-model:items-per-page="itemsPerPage" v-model:page="page" v-model:sort-by="sortBy"
        :headers="headers" :items="items" :items-length="totalItems" :loading="loading" class="elevation-0"
        item-value="id" @update:options="handleUpdateOptions">
        <template #top>
          <v-toolbar flat>
            <v-toolbar-title>{{ t('memberFace.list.title') }}</v-toolbar-title>
            <v-spacer></v-spacer>
            <v-btn v-if="canPerformActions" color="primary" icon @click="emit('create')"
              data-testid="create-member-face-button">
              <v-tooltip :text="t('common.create')">
                <template v-slot:activator="{ props }">
                  <v-icon v-bind="props">mdi-plus</v-icon>
                </template>
              </v-tooltip>
            </v-btn>
            <v-text-field v-model="debouncedSearch" :label="t('common.search')" append-inner-icon="mdi-magnify"
              single-line hide-details clearable class="mr-2"
              data-test-id="member-face-list-search-input"></v-text-field>
          </v-toolbar>
        </template>
        <template v-slot:item.thumbnail="{ item }">
          <div class="d-flex justify-center align-center" style="height: 100%;">
            <v-img v-if="item.thumbnailUrl" rounded :src="item.thumbnailUrl" width="50" height="50" 
              class="my-1"></v-img>
            <v-icon v-else>mdi-image-off</v-icon>
          </div>
        </template>
        <template v-slot:item.memberName="{ item }">
          <MemberName :fullName="item.memberName" :gender="item.memberGender" :avatarUrl="item.memberAvatarUrl" />
        </template>
        <template v-slot:item.familyName="{ item }">
          {{ item.familyName }}
        </template>
        <template v-slot:item.actions="{ item }">
          <v-menu>
            <template v-slot:activator="{ props }">
              <v-btn icon variant="text" size="small" v-bind="props" data-testid="member-face-actions-menu">
                <v-icon>mdi-dots-vertical</v-icon>
              </v-btn>
            </template>
            <v-list>
              <v-list-item @click="emit('view', item)" data-testid="view-member-face-button">
                <v-list-item-title>
                  <v-icon left>mdi-eye</v-icon>
                  {{ t('common.viewDetails') }}
                </v-list-item-title>
              </v-list-item>
              <v-list-item @click="emit('edit', item)" data-testid="edit-member-face-button">
                <v-list-item-title>
                  <v-icon left>mdi-pencil</v-icon>
                  {{ t('common.edit') }}
                </v-list-item-title>
              </v-list-item>
              <v-list-item @click="emit('delete', item)" color="error" data-testid="delete-member-face-button">
                <v-list-item-title>
                  <v-icon left>mdi-delete</v-icon>
                  {{ t('common.delete') }}
                </v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
        </template>
        <template #bottom></template>
      </v-data-table-server>
    </v-card-text>
    <v-card-actions class="d-flex justify-end">
      <v-pagination v-model="page" :length="Math.ceil(totalItems / itemsPerPage)" :total-visible="5"
        rounded="circle"></v-pagination>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberFace } from '@/types';
import MemberName from '@/components/member/MemberName.vue';
import { useAuth } from '@/composables/useAuth';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { DataTableHeader } from 'vuetify'; // NEW

interface MemberFaceListProps {
  items: MemberFace[];
  totalItems: number;
  loading: boolean;
  search?: string; // NEW
  readOnly?: boolean; // NEW
}

const props = defineProps<MemberFaceListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create', 'ai-create', 'update:search']); // NEW emits

const { t } = useI18n();
const { isAdmin, isFamilyManager } = useAuth(); // NEW

const page = ref(1);
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE); // Use constant
const sortBy = ref<any[]>([]);

const searchQuery = ref(props.search); // NEW
let debounceTimer: ReturnType<typeof setTimeout>; // NEW

const debouncedSearch = computed({ // NEW
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

watch(() => props.search, (newSearch) => { // NEW
  if (newSearch !== searchQuery.value) {
    searchQuery.value = newSearch;
  }
});

const canPerformActions = computed(() => { // NEW
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const headers = computed<DataTableHeader[]>(() => [
  { title: t('memberFace.list.headers.thumbnail'), key: 'thumbnail', sortable: false, width: '120px', align: 'center' },
  { title: t('memberFace.list.headers.memberName'), key: 'memberName' },
  { title: t('memberFace.list.headers.familyName'), key: 'familyName' },
  { title: t('memberFace.list.headers.actions'), key: 'actions', sortable: false, width: '120px', align: 'center' },
]);


watch([page, itemsPerPage, sortBy, debouncedSearch], () => { // ADD debouncedSearch
  emit('update:options', {
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value,
    search: debouncedSearch.value, // Pass search query
  });
}, { deep: true });


const handleUpdateOptions = (options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[]; search?: string; }) => { // Update interface
  page.value = options.page;
  itemsPerPage.value = options.itemsPerPage;
  sortBy.value = options.sortBy;
  if (options.search !== undefined) {
    searchQuery.value = options.search;
  }
};
</script>