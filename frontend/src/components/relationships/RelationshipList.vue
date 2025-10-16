<template>
  <v-card>
    <v-data-table :headers="headers" :items="relationshipStore.items" :loading="relationshipStore.loading"
      :items-per-page="relationshipStore.itemsPerPage" :total-items="relationshipStore.totalItems"
      @update:options="loadItems" elevation="0">
      <template #top>
        <v-toolbar flat>
          <v-toolbar-title>{{ t('relationship.list.title') }}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn color="primary" icon @click="showAiCreatePopup = true">
            <v-tooltip :text="t('relationship.list.action.aiCreate')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-robot-happy-outline</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <v-btn color="primary" icon @click="$emit('create')">
            <v-tooltip :text="t('relationship.list.action.create')">
              <template v-slot:activator="{ props }">
                <v-icon v-bind="props">mdi-plus</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
        </v-toolbar>
      </template>
      <template v-slot:item.sourceMemberFullName="{ item }">
        <a @click="viewItem(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.sourceMember?.fullName }}
        </a>
      </template>
      <template v-slot:item.targetMemberFullName="{ item }">
        <a @click="viewItem(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
          {{ item.targetMember?.fullName }}
        </a>
      </template>
      <template v-slot:item.type="{ item }">
        {{ getRelationshipTypeTitle(item.type) }}
      </template>
      <template v-slot:item.actions="{ item }">
        <v-tooltip :text="t('relationship.list.action.edit')">
          <template v-slot:activator="{ props }">
            <v-icon small class="mr-2" v-bind="props" @click="editItem(item)">mdi-pencil</v-icon>
          </template>
        </v-tooltip>
        <v-tooltip :text="t('relationship.list.action.delete')">
          <template v-slot:activator="{ props }">
            <v-icon small v-bind="props" @click="deleteItem(item)">mdi-delete</v-icon>
          </template>
        </v-tooltip>
      </template>
    </v-data-table>
  </v-card>
  <NLRelationshipPopup v-model="showAiCreatePopup" @saved="relationshipStore._loadItems()" />
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Relationship } from '@/types';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';
import NLRelationshipPopup from './NLRelationshipPopup.vue';

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const showAiCreatePopup = ref(false);

const headers = computed(() => [
  { title: t('relationship.list.headers.sourceMember'), key: 'sourceMemberFullName', sortable: true, align: 'start' as const },
  { title: t('relationship.list.headers.targetMember'), key: 'targetMemberFullName', sortable: true, align: 'start' as const },
  { title: t('relationship.list.headers.type'), key: 'type', sortable: true, align: 'start' as const },
  {
    title: t('common.actions'), key: 'actions', sortable: false, width: '120px',
    align: 'center' as const,
  },
]);


const emit = defineEmits([ // Added emits
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
  'ai-create',
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
