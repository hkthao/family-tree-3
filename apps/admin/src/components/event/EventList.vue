<template>
  <v-data-table-server
    v-model:items-per-page="itemsPerPage"
    :headers="headers"
    :items="events"
    :items-length="totalEvents"
    :loading="loading"
    item-value="id"
    @update:options="loadEvents"
    elevation="0"
  >
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title data-testid="event-list-title">{{ t('event.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="$emit('ai-create')" data-testid="ai-create-event-button">
          <v-tooltip :text="t('event.list.action.aiCreate')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-robot-happy-outline</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-btn color="primary" icon @click="$emit('create')" data-testid="add-new-event-button">
          <v-tooltip :text="t('event.list.action.create')">
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
          data-test-id="event-list-search-input"
        ></v-text-field>
      </v-toolbar>
    </template>

    <!-- Date column -->
    <template #item.startDate="{ item }">
      {{ formatDate(item.startDate) }}
    </template>

    <!-- Event Name column -->
    <template #item.name="{ item }">
      <a
        @click="$emit('view', item)"
        class="text-primary font-weight-bold text-decoration-underline cursor-pointer"
        data-testid="event-name-link"
      >
        {{ item.name }}
      </a>
    </template>

    <!-- Family column -->
    <template #item.familyId="{ item }">
      <ChipLookup
        :model-value="item.familyId ?? undefined"
        :data-source="familyStore"
        display-expr="name"
        value-expr="id"
        image-expr="avatarUrl"
      />
    </template>

    <!-- Related Members column -->
    <template #item.relatedMembers="{ item }">
      <ChipLookup
        :model-value="item.relatedMembers || []"
        :data-source="memberStore"
        display-expr="fullName"
        value-expr="id"
        image-expr="avatarUrl"
      />
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <v-tooltip :text="t('event.list.action.edit')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="editEvent(item)" data-testid="edit-event-button">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-tooltip :text="t('event.list.action.delete')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="confirmDelete(item)" data-testid="delete-event-button">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import type { DataTableHeader } from 'vuetify';
import { useMemberStore } from '@/stores/member.store';
import { useFamilyStore } from '@/stores/family.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import ChipLookup from '@/components/common/ChipLookup.vue';
import { formatDate } from '@/utils/dateUtils';

const props = defineProps<{
  events: Event[];
  totalEvents: number;
  loading: boolean;
  search: string;
}>();

const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
  'ai-create',
  'update:search',
]);

const { t } = useI18n();
const memberStore = useMemberStore();
const familyStore = useFamilyStore();

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

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const headers = computed<DataTableHeader[]>(() => [
  {
    title: t('event.list.headers.date'),
    key: 'startDate',
    width: '120px',
    align: 'center',
  },
  {
    title: t('event.list.headers.name'),
    key: 'name',
    minWidth: '150px',
    align: 'start',
    class: 'text-truncate',
  },
  {
    title: t('event.list.headers.family'),
    key: 'familyId',
    width: '150px',
    align: 'start',
    sortable: false,
  },
  {
    title: t('event.list.headers.relatedMembers'),
    key: 'relatedMembers',
    width: 'auto',
    align: 'start',
    sortable: false,
  },
  {
    title: t('event.list.headers.location'),
    key: 'location',
    width: '150px',
    align: 'start',
  },
  {
    title: t('event.list.headers.actions'),
    key: 'actions',
    sortable: false,
    width: '120px',
    align: 'center',
  },
]);

const loadEvents = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: string | string[] | null;
}) => {
  emit('update:options', options);
};

const editEvent = (event: Event) => {
  emit('edit', event);
};

const confirmDelete = (event: Event) => {
  emit('delete', event);
};
</script>
