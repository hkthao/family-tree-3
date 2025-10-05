<template>
  <v-card>
    <v-card-title>{{ t('relationship.list.title') }}</v-card-title>
    <v-card-text>
      <v-data-table
        :headers="headers"
        :items="relationshipStore.items"
        :loading="relationshipStore.loading"
        :items-per-page="relationshipStore.itemsPerPage"
        :total-items="relationshipStore.totalItems"
        @update:options="loadItems"
      >
        <template v-slot:item.actions="{ item }">
          <v-icon small class="mr-2" @click="editItem(item)">mdi-pencil</v-icon>
          <v-icon small @click="deleteItem(item)">mdi-delete</v-icon>
        </template>
      </v-data-table>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Relationship } from '@/types';

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const headers = computed(() => [
  { title: t('relationship.list.headers.sourceMember'), key: 'sourceMemberId' },
  { title: t('relationship.list.headers.targetMember'), key: 'targetMemberId' },
  { title: t('relationship.list.headers.type'), key: 'type' },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

const loadItems = async ({ page, itemsPerPage, sortBy }: { page: number; itemsPerPage: number; sortBy: any[] }) => {
  await relationshipStore.setPage(page);
  await relationshipStore.setItemsPerPage(itemsPerPage);
  // You might need to implement sorting in your store
  await relationshipStore._loadItems();
};

const editItem = (item: Relationship) => {
  // Navigate to edit page
  console.log('Edit item', item);
};

const deleteItem = (item: Relationship) => {
  // Show confirmation and delete
  console.log('Delete item', item);
};
</script>
