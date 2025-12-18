<template>
  <v-card data-testid="member-relationships">
    <v-card-text>
      <v-data-table
        :headers="headers"
        :items="formattedRelationships"
        :loading="isLoading"
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
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Relationship } from '@/types';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';
import { useMemberRelationships } from '@/composables/member/useMemberRelationships'; // Import the new composable

const props = defineProps<{
  memberId: string;
  familyId: string; // Add familyId prop
}>();

const emit = defineEmits([
  'view-member',
]);

const { t } = useI18n();
const { relationships: filteredRelationships, isLoading } = useMemberRelationships(props.memberId, props.familyId);

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



const handleRelationshipClick = (event: MouseEvent) => {
  const target = event.target as HTMLElement;
  if (target.tagName === 'A' && target.dataset.memberId) {
    emit('view-member', target.dataset.memberId);
  }
};
</script>
