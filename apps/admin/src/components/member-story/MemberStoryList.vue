<template>
  <v-card flat>
    <v-card-title class="d-flex align-center">
      <span class="text-h6">{{ t('memberStory.list.title') }}</span>
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
        :items="memberStories"
        :items-length="totalMemberStories"
        :loading="loading"
        @update:options="loadMemberStories"
        class="elevation-0"
      >
        <template v-slot:item.title="{ item }">
          <router-link :to="{ name: 'MemberStoryDetail', params: { memberStoryId: item.id } }">
            {{ item.title }}
          </router-link>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn icon flat small @click="viewMemberStory(item)" color="info" data-testid="view-member-story-button">
            <v-icon>mdi-eye</v-icon>
          </v-btn>
          <v-btn icon flat small @click="editMemberStory(item)" color="warning" data-testid="edit-member-story-button">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn icon flat small @click="confirmDeleteMemberStory(item)" color="error" data-testid="delete-member-story-button">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
        <template v-slot:no-data>
          <v-alert :value="true" color="info" icon="mdi-information">
            {{ t('memberStory.list.noMemberStoriesFound') }}
          </v-alert>
        </template>
      </v-data-table-server>
    </v-card-text>
  </v-card>

  <!-- MemberStory Detail Drawer -->
  <BaseCrudDrawer v-model="detailMemberStoryDrawer" @close="closeDetailMemberStory" width="800">
    <MemberStoryDetail v-if="detailMemberStoryDrawer && selectedMemberStoryId" :member-story-id="selectedMemberStoryId" @close="closeDetailMemberStory" />
  </BaseCrudDrawer>

  <!-- Edit MemberStory Drawer -->
  <BaseCrudDrawer v-model="editMemberStoryDrawer" @close="closeEditMemberStory">

  </BaseCrudDrawer>

  <!-- Delete Confirmation Dialog -->
  <ConfirmDialog
    v-model="deleteConfirmDialog"
    :title="t('memberStory.delete.confirmTitle')"
    :message="t('memberStory.delete.confirmMessage', { title: selectedMemberStory?.title })"
    @confirm="deleteMemberStory"
    @cancel="cancelDeleteMemberStory"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
// import { useRouter } from 'vue-router'; // Removed unused import
import { useMemberStoryStore } from '@/stores/memberStory.store'; // Updated
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import ConfirmDialog from '@/components/common/ConfirmDialog.vue';
import MemberStoryDetail from './MemberStoryDetail.vue'; // Updated

import type { MemberStoryDto } from '@/types/memberStory.d'; // Updated

interface Props {
  memberId: string;
}
const props = defineProps<Props>();

const { t } = useI18n();
const memoryStore = useMemberStoryStore();
const search = ref('');
const memberStories = ref<MemberStoryDto[]>([]); 
const totalMemberStories = ref(0);
const loading = ref(false);
const itemsPerPage = ref(10);
const detailMemberStoryDrawer = ref(false);
const editMemberStoryDrawer = ref(false);
const deleteConfirmDialog = ref(false);
const selectedMemberStoryId = ref<string | undefined>(undefined);
const selectedMemberStory = ref<MemberStoryDto | null>(null); 

const headers = ref([
  { title: t('memberStory.list.header.title'), key: 'title' },
  { title: t('memberStory.list.header.actions'), key: 'actions', sortable: false },
]);



  const loadMemberStories = async (options: {
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
  
  memberStories.value = memoryStore.list.items;
  totalMemberStories.value = memoryStore.list.totalItems;
  loading.value = false;
};
const viewMemberStory = (item: MemberStoryDto) => {
  selectedMemberStoryId.value = item.id;
  detailMemberStoryDrawer.value = true;
};

const editMemberStory = (item: MemberStoryDto) => {
  selectedMemberStoryId.value = item.id;
  editMemberStoryDrawer.value = true;
};

const confirmDeleteMemberStory = (item: MemberStoryDto) => {
  selectedMemberStory.value = item;
  selectedMemberStoryId.value = item.id;
  deleteConfirmDialog.value = true;
};

const closeDetailMemberStory = () => {
  detailMemberStoryDrawer.value = false;
  selectedMemberStoryId.value = undefined;
};

const closeEditMemberStory = () => {
  editMemberStoryDrawer.value = false;
  selectedMemberStoryId.value = undefined;
};


const deleteMemberStory = async () => {
  if (selectedMemberStoryId.value) {
    const result = await memoryStore.deleteItem(selectedMemberStoryId.value);
    if (result.ok) {
      loadMemberStories({ page: 1, itemsPerPage: itemsPerPage.value, sortBy: [] }); // Reload list
      selectedMemberStoryId.value = undefined;
      selectedMemberStory.value = null;
    }
  }
};

const cancelDeleteMemberStory = () => {
  selectedMemberStoryId.value = undefined;
  selectedMemberStory.value = null;
};



onMounted(() => {
  loadMemberStories({ page: 1, itemsPerPage: itemsPerPage.value, sortBy: [] });
});

watch(
  () => props.memberId,
  () => {
    loadMemberStories({ page: 1, itemsPerPage: itemsPerPage.value, sortBy: [] });
  }
);
</script>

<style scoped>
/* Scoped styles for MemoryList */
</style>
