<template>
  <div v-if="parsedResult">
    <h3 class="text-center my-4">{{ t('naturalLanguage.editor.parsedResultTitle') }}</h3>
    <v-row>
      <!-- Members Column - Displays 2 columns on medium screens and up, 1 column on smaller screens -->
      <v-col cols="12" md="6" class="d-flex" v-for="(member, index) in sortedMembers" :key="`member-${index}`">
        <ParsedResultCard
          :item="member"
          type="member"
          :all-members="sortedMembers"
          :serial-number="index + 1"
          @delete="deleteMember(index)"
          @save-item="saveMember(member, index)"
        />
      </v-col>
    </v-row>

    <v-row>
      <!-- Events Column - Displays 2 columns on medium screens and up, 1 column on smaller screens -->
      <v-col cols="12" md="6" class="d-flex" v-for="(event, index) in parsedResult.events" :key="`event-${index}`">
        <ParsedResultCard
          :item="event"
          type="event"
          :all-members="sortedMembers"
          :serial-number="index + 1"
          @delete="deleteEvent(index)"
          @save-item="saveEvent(event, index)"
        />
      </v-col>
    </v-row>
  </div>

</template>

<script setup lang="ts">
import ParsedResultCard from './ParsedResultCard.vue';
import type { AnalyzedDataDto, MemberDataDto, EventDataDto } from '@/types/natural-language.d';
import { useI18n } from 'vue-i18n';
import { computed } from 'vue';

const props = defineProps<{
  parsedResult: AnalyzedDataDto | null;
}>();

const emit = defineEmits(['delete-member', 'delete-event', 'save-member', 'save-event']);

const { t } = useI18n();

const sortedMembers = computed(() => {
  if (!props.parsedResult || !props.parsedResult.members) {
    return [];
  }

  // Sort members: those without relationships first, then those with relationships
  return [...props.parsedResult.members].sort((a, b) => {
    const hasRelationshipsA = a.fatherId || a.motherId || a.husbandId || a.wifeId;
    const hasRelationshipsB = b.fatherId || b.motherId || b.husbandId || b.wifeId;

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

const saveMember = (member: MemberDataDto, index: number) => {
  emit('save-member', member, index);
};

const saveEvent = (event: EventDataDto, index: number) => {
  emit('save-event', event, index);
};
</script>
