<template>
  <v-container fluid>

    <v-data-table-server v-model:items-per-page="memoryStore.list.itemsPerPage" :headers="headers"
      :items="memoryStore.list.items" :items-length="memoryStore.list.totalItems" :loading="memoryStore.list.loading"
      @update:options="handleListOptionsUpdate" item-value="id" class="elevation-0">
      <template #top>
        <v-toolbar flat>
          <v-toolbar-title>{{ t('memory.list.title') }}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-tooltip :text="t('memory.list.action.create')" location="bottom">
            <template v-slot:activator="{ props }">
              <v-btn color="primary" class="mr-2" v-bind="props" @click="openAddDrawer()" variant="text" icon>
                <v-icon>mdi-plus</v-icon>
              </v-btn>
            </template>
          </v-tooltip>
          <v-text-field v-model="searchQuery" class="mr-2" append-inner-icon="mdi-magnify" :label="t('common.search')"
            single-line hide-details></v-text-field>
        </v-toolbar>
      </template>

      <template v-slot:item.title="{ item }">
        <a @click="openDetailDrawer(item.id)"
          class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.title }}
        </a>
      </template>
      <template v-slot:item.memberName="{ item }">
        {{ item.memberId || t('common.unknown') }}
      </template>
      <template v-slot:item.tags="{ item }">
        <v-chip v-for="tag in item.tags" :key="tag" size="small" class="mr-1 my-1">{{ tag }}</v-chip>
      </template>
      <template v-slot:item.createdAt="{ item }">
        {{ formatDate(item.createdAt) }}
      </template>
      <template v-slot:item.actions="{ item }">
        <v-menu>
          <template v-slot:activator="{ props: menuProps }">
            <v-btn icon variant="text" v-bind="menuProps" size="small">
              <v-icon>mdi-dots-vertical</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item @click="openEditDrawer(item.id)">
              <v-list-item-title>{{ t('common.edit') }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="confirmDelete(item)">
              <v-list-item-title>{{ t('common.delete') }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </template>
      <!-- No data message can be handled by the table itself or left empty if not needed -->
    </v-data-table-server>

    <!-- Add Memory Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleCrudDrawerClosed">
      <MemoryAddView v-if="addDrawer" @close="handleCrudDrawerClosed" @saved="handleCrudDrawerSaved" />
    </BaseCrudDrawer>

    <!-- Edit Memory Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleCrudDrawerClosed">
      <MemoryEditView v-if="selectedItemId && editDrawer" :memory-id="selectedItemId" @close="handleCrudDrawerClosed"
        @saved="handleCrudDrawerSaved" />
    </BaseCrudDrawer>

    <!-- Detail Memory Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleCrudDrawerClosed">
      <MemoryDetailView v-if="selectedItemId && detailDrawer" :memory-id="selectedItemId"
        @close="handleCrudDrawerClosed" @edit-item="openEditDrawer" />
    </BaseCrudDrawer>
  </v-container>
</template>

<script setup lang="ts">
import { watch, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useCrudDrawer } from '@/composables/useCrudDrawer';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useMemoryStore } from '@/stores/memory.store'; // Use the new memory store
import type { MemoryDto } from '@/types/memory'; // Import MemoryDto
import MemoryAddView from './MemoryAddView.vue'; // Will be created
import MemoryEditView from './MemoryEditView.vue'; // Will be created
import MemoryDetailView from './MemoryDetailView.vue'; // Will be created
import i18n from '@/plugins/i18n'; // Import i18n instance

interface MemoryListViewProps {
  memberId?: string; // The member ID to list memories for
}

const props = defineProps<MemoryListViewProps>();

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { headers } = storeToRefs(memoryStore); // Get headers from the memory store
const searchQuery = ref('');
const showNoMemberSelectedMessage = ref(false); // New: to control visibility of "no member selected" message

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer: _openAddDrawer, // Rename to avoid conflict
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

// New: Wrapper for openAddDrawer to check for memberId
const openAddDrawer = () => {
  _openAddDrawer();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  memoryStore.setListOptions(options);
};

const handleCrudDrawerClosed = () => {
  closeAllDrawers();
};

const handleCrudDrawerSaved = () => {
  closeAllDrawers();
  memoryStore._loadItems();
};

const confirmDelete = async (memory: MemoryDto) => {
  const confirmed = await showConfirmDialog({
    title: t('memory.delete.confirmTitle'),
    message: t('memory.delete.confirmMessage', { title: memory.title }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    await handleDeleteConfirm(memory);
  }
};

const handleDeleteConfirm = async (memory: MemoryDto) => {
  if (memory) {
    await memoryStore.deleteItem(memory.id);
    if (memoryStore.error) {
      showSnackbar(t('memory.delete.error', { error: memoryStore.error }), 'error');
    } else {
      showSnackbar(t('memory.delete.success'), 'success');
    }
  }
  memoryStore._loadItems();
};

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString(i18n.global.locale.value);
};

// Watch for changes in searchQuery to update filters and reload items
watch(searchQuery, (newSearchQuery) => {
    memoryStore.setFilters({ searchQuery: newSearchQuery, memberId: props.memberId });
    memoryStore._loadItems();
});

// Watch for changes in memberId prop to update filters and reload items
watch(() => props.memberId, (newMemberId) => {
  if (newMemberId) {
    showNoMemberSelectedMessage.value = false;
    memoryStore.setFilters({ memberId: newMemberId });
    memoryStore._loadItems();
  } else {
    memoryStore.setFilters({ memberId: undefined }); // Clear filter
    memoryStore.list.items = []; // Clear current items
    memoryStore.list.totalItems = 0;
    showNoMemberSelectedMessage.value = true;
  }
});

onMounted(() => {
  if (props.memberId) {
    showNoMemberSelectedMessage.value = false;
    memoryStore.setFilters({ memberId: props.memberId });
    memoryStore._loadItems();
  } else {
    showNoMemberSelectedMessage.value = true;
  }
});

</script>
