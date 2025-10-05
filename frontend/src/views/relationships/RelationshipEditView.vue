<template>
  <RelationshipForm :id="id" @save="save" @cancel="cancel" />
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';

import { useRelationshipStore } from '@/stores/relationship.store';

import type { Relationship } from '@/types';

import RelationshipForm from '@/components/relationships/RelationshipForm.vue';

const props = defineProps<{ id: string }>();
const router = useRouter();
const relationshipStore = useRelationshipStore();

const save = async (relationship: Relationship) => {
  await relationshipStore.updateItem(relationship);
  router.push({ name: 'relationship-list' });
};

const cancel = () => {
  router.push({ name: 'relationship-list' });
};
</script>
