<template>
  <v-card>
    <v-card-text>
      <v-data-table
        :headers="headers"
        :items="formattedRelationships"
        :loading="relationshipStore.loading"
        :no-data-text="t('common.noData')"
      >
        <template v-slot:item.formattedRelationship="{ item }">
          <span v-html="item.formattedRelationship" @click="handleRelationshipClick($event)"></span>
        </template>
      </v-data-table>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Relationship } from '@/types';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

const props = defineProps<{
  memberId: string;
}>();

const emit = defineEmits([
  'view-member',
]);

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const headers = computed(() => [
  { title: t('relationship.list.headers.relationship'), key: 'formattedRelationship', sortable: true, align: 'start' as const },
]);

const formattedRelationships = computed(() => {
  return filteredRelationships.value.map((item: Relationship) => ({
    ...item,
    formattedRelationship: `
      <a class="text-primary font-weight-bold text-decoration-underline cursor-pointer" data-member-id="${item.sourceMemberId}">${item.sourceMember?.fullName}</a>
      ${t('relationship.isThe')} ${getRelationshipTypeTitle(item.type)} ${t('relationship.of')} 
      <a class="text-primary font-weight-bold text-decoration-underline cursor-pointer" data-member-id="${item.targetMemberId}">${item.targetMember?.fullName}</a>
    `,
  }));
});

const filteredRelationships = computed(() => {
  if (!props.memberId) return [];
  return relationshipStore.items.filter(
    (rel: Relationship) =>
      rel.sourceMemberId === props.memberId || rel.targetMemberId === props.memberId
  );
});

onMounted(async () => {
  // Fetch all relationships by setting a high itemsPerPage value.
  // This is a workaround. A dedicated API endpoint would be better.
  await relationshipStore.setItemsPerPage(1000);
});

onUnmounted(() => {
  // Reset itemsPerPage to default to not affect other components
  relationshipStore.setItemsPerPage(DEFAULT_ITEMS_PER_PAGE);
});

const handleRelationshipClick = (event: MouseEvent) => {
  const target = event.target as HTMLElement;
  if (target.tagName === 'A' && target.dataset.memberId) {
    emit('view-member', target.dataset.memberId);
  }
};
</script>
