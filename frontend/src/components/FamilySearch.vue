<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-text-field
          v-model="searchQuery"
          label="Search by Name or ID"
          clearable
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
          @input="filterMembers"
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-list lines="three">
          <v-list-item
            v-for="member in filteredMembers"
            :key="member.id"
            :title="member.fullName"
            :subtitle="getMemberSubtitle(member)"
            @click="goToMemberDetail(member.id)"
          >
            <template v-slot:prepend>
              <v-avatar color="grey-lighten-1">
                <v-icon>mdi-account</v-icon>
              </v-avatar>
            </template>
          </v-list-item>
        </v-list>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { mockFamilyMembers } from '@/data/mockFamilyData';
import type { FamilyMember } from '@/data/mockFamilyData';

const searchQuery = ref('');
const router = useRouter();

const filteredMembers = computed(() => {
  if (!searchQuery.value) {
    return mockFamilyMembers.value;
  }
  const query = searchQuery.value.toLowerCase();
  return mockFamilyMembers.value.filter(
    (member) =>
      member.fullName.toLowerCase().includes(query) ||
      member.id.toLowerCase().includes(query)
  );
});

const getMemberSubtitle = (member: FamilyMember) => {
  const birthYear = member.dateOfBirth ? new Date(member.dateOfBirth).getFullYear() : 'N/A';
  // Simplified relationship display for list view
  const relationships = member.relationships
    .map(rel => `${rel.type} (${mockFamilyMembers.value.find(m => m.id === rel.memberId)?.fullName || 'Unknown'})`)
    .join(', ');
  
  return `Born: ${birthYear} | Relationships: ${relationships || 'None'}`;
};

const goToMemberDetail = (id: string) => {
  router.push({ name: 'MemberDetail', params: { id } });
};

const filterMembers = () => {
  // Filtering is handled by computed property, no direct action needed here.
};
</script>

<style scoped>
/* Add any specific styles for this component here */
</style>