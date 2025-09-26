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
        <v-toolbar-title>{{ t('event.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="$emit('create')">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
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
      <v-btn icon size="small" variant="text" @click="editEvent(item)">
        <v-icon>mdi-pencil</v-icon>
      </v-btn>
      <v-btn icon size="small" variant="text" @click="confirmDelete(item)">
        <v-icon>mdi-delete</v-icon>
      </v-btn>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types/event/event';
import type { DataTableHeader } from 'vuetify';
import { useMemberStore } from '@/stores/member.store';
import { useFamilyStore } from '@/stores/family.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import ChipLookup from '@/components/common/ChipLookup.vue';
import { formatDate } from '@/utils/dateUtils';

defineProps({
  events: {
    type: Array as () => Event[],
    required: true,
  },
  totalEvents: {
    type: Number,
    required: true,
  },
  loading: {
    type: Boolean,
    required: true,
  },
});

const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
]);

const { t } = useI18n();
const memberStore = useMemberStore();
const familyStore = useFamilyStore();

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
