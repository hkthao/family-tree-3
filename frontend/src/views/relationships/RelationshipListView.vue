<template>
  <div>
    <RelationshipSearch @update:filters="updateFilters" />
    <RelationshipList />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

import { useRelationshipStore } from '@/stores/relationship.store';

import type { RelationshipFilter } from '@/types';

import RelationshipList from '@/components/relationships/RelationshipList.vue';
import RelationshipSearch from '@/components/relationships/RelationshipSearch.vue';

const relationshipStore = useRelationshipStore();

const updateFilters = (filters: RelationshipFilter) => {
  relationshipStore.filter = filters;
  relationshipStore._loadItems();
};
</script>
