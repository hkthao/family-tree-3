<template>
  <div v-if="parsedResult">
    <h3 class="text-center my-4">{{ t('naturalLanguage.editor.parsedResultTitle') }}</h3>
    <v-row>
      <!-- Members Column -->
      <v-col cols="12" md="6" class="d-flex" v-for="(member, index) in sortedMembers" :key="`member-${index}`">
        <ParsedMemberCard
          :member="member"
          :all-members="sortedMembers"
          :all-relationships="parsedResult.relationships"
          :serial-number="index + 1"
          @delete="deleteMember(index)"
          @save-member="saveMember(member, index)"
        />
      </v-col>
    </v-row>

     <v-row>
      <!-- Relationships Column -->
      <v-col cols="12" md="6" class="d-flex" v-for="(relationship, index) in parsedResult.relationships" :key="`relationship-${index}`">
        <ParsedRelationshipCard
          :relationship="relationship"
          :all-members="sortedMembers"
          :serial-number="index + 1"
          @delete="deleteRelationship(index)"
          @save-relationship="saveRelationship(relationship, index)"
        />
      </v-col>
    </v-row>

    <v-row>
      <!-- Events Column -->
      <v-col cols="12" md="6" class="d-flex" v-for="(event, index) in parsedResult.events" :key="`event-${index}`">
        <ParsedEventCard
          :event="event"
          :serial-number="index + 1"
          @delete="deleteEvent(index)"
          @save-event="saveEvent(event, index)"
        />
      </v-col>
    </v-row>
   
  </div>
</template>

<script setup lang="ts">
import ParsedMemberCard from './ParsedMemberCard.vue';
import ParsedEventCard from './ParsedEventCard.vue';
import ParsedRelationshipCard from './ParsedRelationshipCard.vue';
import type { AnalyzedDataDto, MemberDataDto, EventDataDto, RelationshipDataDto } from '@/types/natural-language.d';
import { useI18n } from 'vue-i18n';
import { computed } from 'vue';

const props = defineProps<{
  parsedResult: AnalyzedDataDto | null;
}>();

const emit = defineEmits([
  'delete-member',
  'delete-event',
  'delete-relationship', // New emit
  'save-member',
  'save-event',
  'save-relationship', // New emit
]);

const { t } = useI18n();

const sortedMembers = computed(() => {
  if (!props.parsedResult || !props.parsedResult.members) {
    return [];
  }

  const relationships = props.parsedResult.relationships || [];

  // Helper function to check if a member has any relationships
  const memberHasRelationships = (memberId: string) => {
    return relationships.some(
      (rel) => rel.sourceMemberId === memberId || rel.targetMemberId === memberId
    );
  };

  // Sort members: those without relationships first, then those with relationships
  return [...props.parsedResult.members].sort((a, b) => {
    const hasRelationshipsA = memberHasRelationships(a.id || '');
    const hasRelationshipsB = memberHasRelationships(b.id || '');

    if (!hasRelationshipsA && hasRelationshipsB) {
      return -1; // A comes before B
    }
    if (hasRelationshipsA && !hasRelationshipsB) {
      return 1; // B comes before A
    }
    return 0; // Maintain original order if both have/don't have relationships
  });
});

const deleteMember = (index: number) => {
  emit('delete-member', index);
};

const deleteEvent = (index: number) => {
  emit('delete-event', index);
};

const deleteRelationship = (index: number) => {
  emit('delete-relationship', index);
};

const saveMember = (member: MemberDataDto, index: number) => {
  emit('save-member', member, index);
};

const saveEvent = (event: EventDataDto, index: number) => {
  emit('save-event', event, index);
};

const saveRelationship = (relationship: RelationshipDataDto, index: number) => {
  emit('save-relationship', relationship, index);
};
</script>
