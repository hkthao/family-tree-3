<template>
  <v-container fluid>

    <v-data-table-server v-model:items-per-page="memberStoryStore.list.itemsPerPage" :headers="headers"
      :items="memberStoryStore.list.items" :items-length="memberStoryStore.list.totalItems" :loading="memberStoryStore.list.loading"
      @update:options="handleListOptionsUpdate" item-value="id" class="elevation-0">
      <template #top>
        <v-toolbar flat>
          <v-toolbar-title>{{ t('memberStory.list.title') }}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-tooltip :text="t('memberStory.list.action.create')" location="bottom">
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
        <a @click="openDetailDrawer(item.id!)"
          class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.title }}
        </a>
      </template>
      <template v-slot:item.memberName="{ item }">
        {{ item.memberId || t('common.unknown') }}
      </template>
      <template v-slot:item.actions="{ item }">
        <v-menu>
          <template v-slot:activator="{ props: menuProps }">
            <v-btn icon variant="text" v-bind="menuProps" size="small">
              <v-icon>mdi-dots-vertical</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item @click="openEditDrawer(item.id!)">
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

    <!-- Add MemberStory Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleCrudDrawerClosed">
      <MemberStoryAddView v-if="addDrawer" @close="handleCrudDrawerClosed" @saved="handleCrudDrawerSaved" />
    </BaseCrudDrawer>

    <!-- Edit MemberStory Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleCrudDrawerClosed">
      <MemberStoryEditView v-if="selectedItemId && editDrawer" :member-story-id="selectedItemId" @close="handleCrudDrawerClosed"
        @saved="handleCrudDrawerSaved" />
    </BaseCrudDrawer>

    <!-- Detail MemberStory Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleCrudDrawerClosed">
      <MemberStoryDetailView v-if="selectedItemId && detailDrawer" :member-story-id="selectedItemId"
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
import { useMemberStoryStore } from '@/stores/memberStory.store'; // Use the new member story store // Updated
import type { MemberStoryDto } from '@/types/memberStory'; // Import MemberStoryDto // Updated
import MemberStoryAddView from './MemberStoryAddView.vue'; // Will be created // Updated
import MemberStoryEditView from './MemberStoryEditView.vue'; // Will be created // Updated
import MemberStoryDetailView from './MemberStoryDetailView.vue'; // Will be created // Updated


interface MemberStoryListViewProps {
  memberId?: string; // The member ID to list member stories for // Updated
}

const props = defineProps<MemberStoryListViewProps>();

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore(); // Updated
const { headers } = storeToRefs(memberStoryStore); // Get headers from the member story store // Updated
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
  memberStoryStore.setListOptions(options); // Updated
};

const handleCrudDrawerClosed = () => {
  closeAllDrawers();
};

const handleCrudDrawerSaved = () => {
  closeAllDrawers();
  memberStoryStore._loadItems(); // Updated
};

const confirmDelete = async (memberStory: MemberStoryDto) => { // Updated
  const confirmed = await showConfirmDialog({
    title: t('memberStory.delete.confirmTitle'), // Updated
    message: t('memberStory.delete.confirmMessage', { title: memberStory.title }), // Updated
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    await handleDeleteConfirm(memberStory);
  }
};

const handleDeleteConfirm = async (memberStory: MemberStoryDto) => { // Updated
  if (memberStory) {
    await memberStoryStore.deleteItem(memberStory.id!); // Use non-null assertion as memberStory.id is expected for existing member story // Updated
    if (memberStoryStore.error) { // Updated
      showSnackbar(t('memberStory.delete.error', { error: memberStoryStore.error }), 'error'); // Updated
    } else {
      showSnackbar(t('memberStory.delete.success'), 'success'); // Updated
    }
  }
  memberStoryStore._loadItems(); // Updated
};



// Watch for changes in searchQuery to update filters and reload items
watch(searchQuery, (newSearchQuery) => {
    memberStoryStore.setFilters({ searchQuery: newSearchQuery, memberId: props.memberId }); // Updated
    memberStoryStore._loadItems(); // Updated
});

// Watch for changes in memberId prop to update filters and reload items
watch(() => props.memberId, (newMemberId) => {
  if (newMemberId) {
    showNoMemberSelectedMessage.value = false;
    memberStoryStore.setFilters({ memberId: newMemberId }); // Updated
    memberStoryStore._loadItems(); // Updated
  } else {
    memberStoryStore.setFilters({ memberId: undefined }); // Clear filter // Updated
    memberStoryStore.list.items = []; // Clear current items // Updated
    memberStoryStore.list.totalItems = 0; // Updated
    showNoMemberSelectedMessage.value = true;
  }
});

onMounted(() => {
  if (props.memberId) {
    showNoMemberSelectedMessage.value = false;
    memberStoryStore.setFilters({ memberId: props.memberId }); // Updated
    memberStoryStore._loadItems(); // Updated
  } else {
    showNoMemberSelectedMessage.value = true;
  }
});

</script>
