<template>
  <v-card>
    <v-card-title>{{ t('relationship.list.title') }}</v-card-title>
    <v-card-text>
      <v-data-table :headers="headers" :items="relationshipStore.items" :loading="relationshipStore.loading"
        :items-per-page="relationshipStore.itemsPerPage" :total-items="relationshipStore.totalItems"
        @update:options="loadItems">
        <template v-slot:item.sourceMemberFullName="{ item }">
          <a @click="viewItem(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
            {{ item.sourceMemberFullName }}
          </a>
        </template>
        <template v-slot:item.targetMemberFullName="{ item }">
          <a @click="viewItem(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
            {{ item.targetMemberFullName }}
          </a>
        </template>
        <template v-slot:item.type="{ item }">
          {{ getRelationshipTypeTitle(item.type) }}
        </template>
        <template v-slot:item.actions="{ item }">
          <v-icon small class="mr-2" @click="editItem(item)">mdi-pencil</v-icon>
          <v-icon small @click="deleteItem(item)">mdi-delete</v-icon>
        </template>
      </v-data-table>
    </v-card-text>
  </v-card>

</template>

<script setup lang="ts">
import { computed } from 'vue'; // Removed ref
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Relationship } from '@/types';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const headers = computed(() => [
  { title: t('relationship.list.headers.sourceMember'), key: 'sourceMemberFullName', sortable: true },
  { title: t('relationship.list.headers.targetMember'), key: 'targetMemberFullName', sortable: true },
  { title: t('relationship.list.headers.type'), key: 'type', sortable: true },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);


const emit = defineEmits([ // Added emits
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
]);

const loadItems = async (options: { page: number; itemsPerPage: number; sortBy: any[] }) => {
  emit('update:options', options);
};

const viewItem = (item: Relationship) => {
  emit('view', item);
};

const editItem = (item: Relationship) => {
  emit('edit', item);
};

const deleteItem = (item: Relationship) => {
  emit('delete', item);
};
</script>
