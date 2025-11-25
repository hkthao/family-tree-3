<template>
  <v-card flat>
    <v-card-title class="d-flex align-center">
      <span class="text-h6">{{ t('memory.list.title') }}</span>
      <v-spacer></v-spacer>
      <v-text-field
        v-model="search"
        append-inner-icon="mdi-magnify"
        :label="t('common.search')"
        single-line
        hide-details
        density="compact"
        class="flex-grow-0"
        style="max-width: 200px;"
      ></v-text-field>
    </v-card-title>
    <v-card-text>
      <v-data-table-server
        v-model:items-per-page="itemsPerPage"
        :headers="headers"
        :items="memories"
        :items-length="totalMemories"
        :loading="loading"
        @update:options="loadMemories"
        class="elevation-0"
      >
        <template v-slot:item.title="{ item }">
          <router-link :to="{ name: 'MemoryDetail', params: { memoryId: item.id } }">
            {{ item.title }}
          </router-link>
        </template>
        <template v-slot:item.createdAt="{ item }">
          {{ formatDateTime(item.createdAt) }}
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn icon flat small @click="viewMemory(item)" color="info" data-testid="view-memory-button">
            <v-icon>mdi-eye</v-icon>
          </v-btn>
          <v-btn icon flat small @click="editMemory(item)" color="warning" data-testid="edit-memory-button">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn icon flat small @click="confirmDeleteMemory(item)" color="error" data-testid="delete-memory-button">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
        <template v-slot:no-data>
          <v-alert :value="true" color="info" icon="mdi-information">
            {{ t('memory.list.noMemoriesFound') }}
          </v-alert>
        </template>
      </v-data-table-server>
    </v-card-text>
  </v-card>

  <!-- Memory Detail Drawer -->
  <BaseCrudDrawer v-model="detailMemoryDrawer" @close="closeDetailMemory" width="800">
    <MemoryDetail v-if="detailMemoryDrawer && selectedMemoryId" :memory-id="selectedMemoryId" @close="closeDetailMemory" />
  </BaseCrudDrawer>

  <!-- Edit Memory Drawer -->
  <BaseCrudDrawer v-model="editMemoryDrawer" @close="closeEditMemory">
    <MemoryEdit v-if="editMemoryDrawer && selectedMemoryId" :memory-id="selectedMemoryId" @close="closeEditMemory" @saved="handleMemorySaved" />
  </BaseCrudDrawer>

  <!-- Delete Confirmation Dialog -->
  <ConfirmDialog
    v-model="deleteConfirmDialog"
    :title="t('memory.delete.confirmTitle')"
    :message="t('memory.delete.confirmMessage', { title: selectedMemory?.title })"
    @confirm="deleteMemory"
    @cancel="cancelDelete"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
// import { useRouter } from 'vue-router'; // Removed unused import
import { useMemoryStore } from '@/stores/memory.store';
import { formatDateTime } from '@/utils/formatters';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import ConfirmDialog from '@/components/common/ConfirmDialog.vue';
import MemoryDetail from './MemoryDetail.vue';
import MemoryEdit from './MemoryEdit.vue';
import type { MemoryDto } from '@/types/memory.d'; // Import MemoryDto

interface Props {
  memberId: string;
}
const props = defineProps<Props>();

const { t } = useI18n();
const memoryStore = useMemoryStore();
const search = ref('');
const memories = ref<MemoryDto[]>([]); 
const totalMemories = ref(0);
const loading = ref(false);
const itemsPerPage = ref(10);
const detailMemoryDrawer = ref(false);
const editMemoryDrawer = ref(false);
const deleteConfirmDialog = ref(false);
const selectedMemoryId = ref<string | undefined>(undefined);
const selectedMemory = ref<MemoryDto | null>(null); 

const headers = ref([
  { title: t('memory.list.header.title'), key: 'title' },
  { title: t('memory.list.header.tags'), key: 'tags' },
  { title: t('memory.list.header.createdAt'), key: 'createdAt' },
  { title: t('memory.list.header.actions'), key: 'actions', sortable: false },
]);



  const loadMemories = async (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  loading.value = true;
  memoryStore.list.filters.memberId = props.memberId;
  memoryStore.list.filters.searchQuery = search.value;
  memoryStore.list.currentPage = options.page;
  memoryStore.list.itemsPerPage = options.itemsPerPage;
  memoryStore.list.sortBy = options.sortBy;

  await memoryStore._loadItems();
  
  memories.value = memoryStore.list.items;
  totalMemories.value = memoryStore.list.totalItems;
  loading.value = false;
};
const viewMemory = (item: MemoryDto) => {
  selectedMemoryId.value = item.id;
  detailMemoryDrawer.value = true;
};

const editMemory = (item: MemoryDto) => {
  selectedMemoryId.value = item.id;
  editMemoryDrawer.value = true;
};

const confirmDeleteMemory = (item: MemoryDto) => {
  selectedMemory.value = item;
  selectedMemoryId.value = item.id;
  deleteConfirmDialog.value = true;
};

const closeDetailMemory = () => {
  detailMemoryDrawer.value = false;
  selectedMemoryId.value = undefined;
};

const closeEditMemory = () => {
  editMemoryDrawer.value = false;
  selectedMemoryId.value = undefined;
};


const deleteMemory = async () => {
  if (selectedMemoryId.value) {
    const result = await memoryStore.deleteItem(selectedMemoryId.value);
    if (result.ok) {
      loadMemories({ page: 1, itemsPerPage: itemsPerPage.value, sortBy: [] }); // Reload list
      selectedMemoryId.value = undefined;
      selectedMemory.value = null;
    }
  }
};

const cancelDelete = () => {
  selectedMemoryId.value = undefined;
  selectedMemory.value = null;
};

const handleMemorySaved = () => {
  editMemoryDrawer.value = false;
  loadMemories({ page: 1, itemsPerPage: itemsPerPage.value, sortBy: [] }); // Reload list
};

onMounted(() => {
  loadMemories({ page: 1, itemsPerPage: itemsPerPage.value, sortBy: [] });
});

watch(
  () => props.memberId,
  () => {
    loadMemories({ page: 1, itemsPerPage: itemsPerPage.value, sortBy: [] });
  }
);
</script>

<style scoped>
/* Scoped styles for MemoryList */
</style>
