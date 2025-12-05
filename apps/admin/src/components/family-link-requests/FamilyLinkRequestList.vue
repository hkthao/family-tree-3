<template>
  <v-data-table-server :headers="headers" :items="items" :items-length="totalItems" :loading="loading" item-value="id"
    @update:options="loadItems" elevation="0" fixed-header>
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title class="text-h6">
          {{ t('familyLinkRequest.list.title') }}
        </v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="$emit('create')" :disabled="readOnly"
          data-testid="add-new-family-link-request-button">
          <v-tooltip :text="t('familyLinkRequest.list.action.create')">
            <template v-slot:activator="{ props: tooltipProps }">
              <v-icon v-bind="tooltipProps">mdi-plus</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-text-field v-model="debouncedSearch" :label="t('common.search')" append-inner-icon="mdi-magnify" single-line
          hide-details clearable class="mr-2" data-test-id="family-link-request-list-search-input"></v-text-field>
      </v-toolbar>
    </template>

    <template #item.requestingFamilyName="{ item }">
      <a @click="$emit('view', item.id)"
        class="text-primary font-weight-bold text-decoration-underline cursor-pointer" aria-label="View Family Link Request">
        {{ item.requestingFamilyName }}
      </a>
    </template>

    <template #item.targetFamilyName="{ item }">
      {{ item.targetFamilyName }}
    </template>
    <template #item.status="{ item }">
      <v-chip :color="getStatusColor(item.status)">
        {{ t(`familyLinkRequest.status.${getDisplayStatus(item.status).toLowerCase()}`) }}
      </v-chip>
    </template>
    <template #item.requestDate="{ item }">
      {{ formatDate(item.requestDate) }}
    </template>
    <template #item.responseDate="{ item }">
      {{ item.responseDate ? formatDate(item.responseDate) : t('common.na') }}
    </template>
    <template #item.actions="{ item }">
      <div v-if="!readOnly" class="d-flex justify-center">
        <template v-if="isAdmin">
          <v-tooltip :text="t('common.delete')">
            <template v-slot:activator="{ props }">
              <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('delete', item.id)"
                data-testid="delete-family-link-request-button">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </template>
          </v-tooltip>
        </template>
        <template v-if="item.status === LinkStatus.Pending">
          <v-tooltip :text="t('familyLinkRequest.list.action.approve')">
            <template v-slot:activator="{ props }">
              <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('approve', item.id)"
                data-testid="approve-family-link-request-button">
                <v-icon>mdi-check</v-icon>
              </v-btn>
            </template>
          </v-tooltip>
          <v-tooltip :text="t('familyLinkRequest.list.action.reject')">
            <template v-slot:activator="{ props }">
              <v-btn icon size="small" variant="text" v-bind="props" @click="$emit('reject', item.id)"
                data-testid="reject-family-link-request-button">
                <v-icon>mdi-close</v-icon>
              </v-btn>
            </template>
          </v-tooltip>
        </template>
      </div>
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyLinkRequestDto } from '@/types';
import { LinkStatus } from '@/types';
import type { DataTableHeader } from 'vuetify';
import { formatDate } from '@/utils/dateUtils';
import { useAuth } from '@/composables/useAuth'; // Import useAuth composable

interface FamilyLinkRequestListProps {
  items: FamilyLinkRequestDto[];
  totalItems: number;
  loading: boolean;
  search?: string; // New prop for search
  readOnly?: boolean;
}

const props = defineProps<FamilyLinkRequestListProps>();

const emit = defineEmits([
  'update:options',
  'view',
  'delete',
  'create',
  'approve',
  'reject',
  'update:search',
]);

const { t } = useI18n();
const { isAdmin } = useAuth(); // Destructure isAdmin from useAuth()

const searchQuery = ref(props.search); // Use ref for search input
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

// Watch prop.search to update local searchQuery if prop changes
watch(() => props.search, (newSearch) => {
  if (newSearch !== searchQuery.value) {
    searchQuery.value = newSearch;
  }
});

const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    { title: t('familyLinkRequest.list.headers.requestingFamily'), key: 'requestingFamilyName', sortable: false },
    { title: t('familyLinkRequest.list.headers.targetFamily'), key: 'targetFamilyName', sortable: false },
    { title: t('familyLinkRequest.list.headers.status'), key: 'status', sortable: false },
    { title: t('familyLinkRequest.list.headers.requestDate'), key: 'requestDate', sortable: true, align: 'center' },
    { title: t('familyLinkRequest.list.headers.responseDate'), key: 'responseDate', sortable: true, align: 'center' },
  ];

  if (!props.readOnly) { // Only show actions if not readOnly
    baseHeaders.push({ title: t('common.actions'), key: 'actions', sortable: false, align: 'center' });
  }
  return baseHeaders;
});

const getDisplayStatus = (status: LinkStatus | number): LinkStatus => {
  if (typeof status === 'number') {
    // For string enums, Object.values returns the string values in order.
    // We can use the number as an index to get the corresponding string.
    return Object.values(LinkStatus)[status];
  }
  return status;
};

const getStatusColor = (status: LinkStatus) => {
  switch (status) {
    case LinkStatus.Pending: return 'warning';
    case LinkStatus.Approved: return 'success';
    case LinkStatus.Rejected: return 'error';
    default: return 'info';
  }
};

const loadItems = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  emit('update:options', options);
};
</script>