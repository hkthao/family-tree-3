<template>
  <v-card>
    <v-card-text>
      <v-data-table
        :headers="headers"
        :items="filteredRelationships"
        :loading="relationshipStore.loading"
        :no-data-text="t('common.noData')"
      >
        <template v-slot:item.sourceMemberFullName="{ item }">
          {{ item.sourceMember?.fullName }}
        </template>
        <template v-slot:item.targetMemberFullName="{ item }">
          {{ item.targetMember?.fullName }}
        </template>
        <template v-slot:item.type="{ item }">
          {{ getRelationshipTypeTitle(item.type) }}
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

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const headers = computed(() => [
  { title: t('relationship.list.headers.sourceMember'), key: 'sourceMemberFullName', sortable: true, align: 'start' as const },
  { title: t('relationship.list.headers.targetMember'), key: 'targetMemberFullName', sortable: true, align: 'start' as const },
  { title: t('relationship.list.headers.type'), key: 'type', sortable: true, align: 'start' as const },
]);

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
</script>
