<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('event.timeline.title') }}
    </v-card-title>
    <v-card-text>
      <v-timeline density="compact" side="end" truncate-line="both">
        <v-timeline-item
          v-for="event in events"
          :key="event.id"
          :dot-color="event.color || 'primary'"
          size="small"
        >
          <div class="d-flex justify-space-between flex-wrap">
            <div class="text-h6">{{ event.name }}</div>
            <div class="text-caption text-grey">{{ formatDate(event.startDate) }}</div>
          </div>
          <div class="text-body-2">{{ event.description }}</div>
          <div v-if="event.location" class="text-caption text-grey">
            <v-icon size="small">mdi-map-marker</v-icon> {{ event.location }}
          </div>
          <v-chip-group v-if="event.relatedMembers && event.relatedMembers.length > 0" class="mt-2">
            <v-chip v-for="memberId in event.relatedMembers" :key="memberId" size="small">
              <v-avatar start>
                <v-img :src="getMemberAvatar(memberId)"></v-img>
              </v-avatar>
              {{ getMemberName(memberId) }}
            </v-chip>
          </v-chip-group>
        </v-timeline-item>
      </v-timeline>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyEvent } from '@/types/family';
import type { Member } from '@/types/family';
import { formatDate } from '@/utils/dateUtils';
import { useMemberStore } from '@/stores/member.store';

const { events } = defineProps<{
  events: FamilyEvent[];
}>();

const { t } = useI18n();
const memberStore = useMemberStore();

const allMembers = ref<Member[]>([]);

const getMemberName = (memberId: string) => {
  const member = allMembers.value.find(m => m.id === memberId);
  return member ? member.fullName : 'Unknown';
};

const getMemberAvatar = (memberId: string) => {
  const member = allMembers.value.find(m => m.id === memberId);
  return member ? member.avatarUrl : '';
};

onMounted(async () => {
  await memberStore.fetchItems(); // Fetch all members
  allMembers.value = memberStore.items;
});
</script>

<style scoped>
/* Add any specific styles for the timeline here */
</style>
