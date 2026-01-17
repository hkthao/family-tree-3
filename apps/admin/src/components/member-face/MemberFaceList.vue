<template>
  <v-data-table-server :headers="headers" :items="items" :items-length="totalItems" :loading="loading" class="elevation-0"
    item-value="id" @update:options="props.readOnly ? null : handleUpdateOptions($event)"
    :items-per-page="props.itemsPerPage" :page="props.page" v-model:sort-by="sortBy">
    <template #top>
      <ListToolbar
        :title="t('memberFace.list.title')"
        :search-query="searchQuery"
        @update:search="debouncedSearch = $event"
        :create-button-tooltip="t('common.create')"
        create-button-test-id="create-member-face-button"
        :hide-create-button="props.readOnly"
        @create="emit('create')"
      >
        <template #custom-buttons>
          <v-btn
            v-if="props.canPerformActions && !props.readOnly"
            color="primary"
            icon
            @click="props.onExport()"
            data-testid="export-member-face-button"
            :aria-label="t('common.export')"
            :loading="props.isExporting"
          >
            <v-tooltip :text="t('common.export')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-export</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <v-btn
            v-if="props.canPerformActions && !props.readOnly"
            color="primary"
            icon
            @click="props.onImportClick()"
            data-testid="import-member-face-button"
            :aria-label="t('common.import')"
            :loading="props.isImporting"
          >
            <v-tooltip :text="t('common.import')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-import</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
        </template>
      </ListToolbar>
    </template>
    <template v-slot:item.thumbnail="{ item }">
      <div class="d-flex justify-center align-center" style="height: 100%;">
        <v-img v-if="item.thumbnailUrl" rounded :src="item.thumbnailUrl" width="50" height="50"
          class="my-1"></v-img>
        <v-icon v-else>mdi-image-off</v-icon>
      </div>
    </template>
    <template v-slot:item.memberName="{ item }">
      <div @click="emit('view', item.id)" class="cursor-pointer">
        <MemberName :fullName="item.memberName" :gender="item.memberGender" :avatarUrl="item.memberAvatarUrl" />
      </div>
    </template>
    <template v-slot:item.familyName="{ item }">
      <FamilyName :name="item.familyName" :avatar-url="item.familyAvatarUrl" />
    </template>
    <template v-slot:item.emotion="{ item }">
      <v-chip v-if="item.emotion" color="info" size="small">{{ item.emotion }}</v-chip>
    </template>
    <template v-slot:item.actions="{ item }">
      <v-tooltip :text="t('common.viewDetails')">
        <template v-slot:activator="{ props }">
          <v-btn icon variant="text" size="small" v-bind="props" @click="emit('view', item.id)"
            data-testid="view-member-face-button">
            <v-icon>mdi-eye</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-tooltip :text="t('common.delete')">
        <template v-slot:activator="{ props }">
          <v-btn icon variant="text" size="small" v-bind="props" @click="emit('delete', item)" data-testid="delete-member-face-button">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
    </template> <template #bottom></template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberFace } from '@/types';
import MemberName from '@/components/member/MemberName.vue';
import FamilyName from '@/components/common/FamilyName.vue';
import ListToolbar from '@/components/common/ListToolbar.vue';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { DataTableHeader } from 'vuetify';

interface MemberFaceListProps {
  items: MemberFace[];
  totalItems: number;
  loading: boolean;
  search?: string;
  page?: number; // Added
  itemsPerPage?: number; // Added
  readOnly?: boolean;
  isExporting: boolean;
  isImporting: boolean;
  canPerformActions: boolean;
  onExport: () => void; // Added
  onImportClick: () => void; // Added
}

const props = defineProps<MemberFaceListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create', 'ai-create', 'update:search']);

const { t } = useI18n();


const sortBy = ref<any[]>([]);

const searchQuery = ref(props.search);
let debounceTimer: ReturnType<typeof setTimeout>;

const debouncedSearch = computed({
  get() {
    return searchQuery.value;
  },
  set(newValue: string | undefined) { // Allow undefined for clearable
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

const headers = computed<DataTableHeader[]>(() => [
  { title: t('memberFace.list.headers.thumbnail'), key: 'thumbnail', sortable: false, width: '120px', align: 'center' },
  { title: t('memberFace.list.headers.memberName'), key: 'memberName' },
  { title: t('memberFace.list.headers.familyName'), key: 'familyName' },
  { title: t('memberFace.list.headers.emotion'), key: 'emotion', sortable: false, width: '100px', align: 'center' },
  { title: t('memberFace.list.headers.actions'), key: 'actions', sortable: false, width: '120px', align: 'center' },
]);





const handleUpdateOptions = (options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[]; search?: string; }) => {
  emit('update:options', options);
};
</script>