<template>
  <v-container v-if="member">
    <v-card>
      <v-card-title class="text-h5">{{ member.fullName }}</v-card-title>
      <v-card-subtitle>ID: {{ member.id }}</v-card-subtitle>
      <v-card-text>
        <v-list dense>
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title>Date of Birth:</v-list-item-title>
              <v-list-item-subtitle>{{ member.dateOfBirth || 'N/A' }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item v-if="member.dateOfDeath">
            <v-list-item-content>
              <v-list-item-title>Date of Death:</v-list-item-title>
              <v-list-item-subtitle>{{ member.dateOfDeath }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item v-if="member.placeOfBirth">
            <v-list-item-content>
              <v-list-item-title>Place of Birth:</v-list-item-title>
              <v-list-item-subtitle>{{ member.placeOfBirth }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title>Gender:</v-list-item-title>
              <v-list-item-subtitle>{{ member.gender }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item v-if="member.phone">
            <v-list-item-content>
              <v-list-item-title>Phone:</v-list-item-title>
              <v-list-item-subtitle>{{ member.phone }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item v-if="member.email">
            <v-list-item-content>
              <v-list-item-title>Email:</v-list-item-title>
              <v-list-item-subtitle>{{ member.email }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item v-if="member.generation">
            <v-list-item-content>
              <v-list-item-title>Generation:</v-list-item-title>
              <v-list-item-subtitle>{{ member.generation }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item v-if="member.biography">
            <v-list-item-content>
              <v-list-item-title>Biography:</v-list-item-title>
              <v-list-item-subtitle>{{ member.biography }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title>Relationships:</v-list-item-title>
              <v-list-item-subtitle>
                <ul v-if="member.relationships && member.relationships.length">
                  <li v-for="(rel, index) in member.relationships" :key="index">
                    {{ rel.type }} - {{ getMemberFullName(rel.memberId) }}
                  </li>
                </ul>
                <span v-else>None</span>
              </v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-list>
      </v-card-text>
      <v-card-actions>
        <v-btn color="primary" @click="viewFamilyTree">View Family Tree</v-btn>
        <v-btn color="secondary" @click="goBack">Back to Search</v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
  <v-container v-else>
    <v-alert type="error">Member not found.</v-alert>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { mockFamilyMembers } from '@/data/mockFamilyData';
import type { FamilyMember } from '@/data/mockFamilyData';

const route = useRoute();
const router = useRouter();
const member = ref<FamilyMember | undefined>(undefined);

onMounted(() => {
  const memberId = route.params.id as string;
  member.value = mockFamilyMembers.value.find((m) => m.id === memberId);
});

const getMemberFullName = (id: string) => {
  return mockFamilyMembers.value.find(m => m.id === id)?.fullName || 'Unknown';
};

const viewFamilyTree = () => {
  alert('Navigating to Family Tree View (Not implemented)');
  // Implement actual navigation to family tree view
};

const goBack = () => {
  router.back();
};
</script>

<style scoped>
/* Add any specific styles for this component here */
</style>