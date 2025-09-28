<template>
  <v-container fluid>
    <v-card fluid class="fill-height">
      <v-row class="fill-height">
        <v-col cols="12" class="d-flex flex-column">
          <v-toolbar dense flat>
            <v-toolbar-title>Cây Gia Phả</v-toolbar-title>
            <v-spacer></v-spacer>
            <FamilyAutocomplete
              class="mr-2 mt-4 fa-filter"
              label="Lọc theo gia đình..."
              v-model="selectedFamilyId"
            />
          </v-toolbar>
          <FamilyTreeChart :familyId="selectedFamilyId" />
        </v-col>
      </v-row>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import FamilyTreeChart from '@/components/family/FamilyTreeChart.vue';
const selectedFamilyId = ref<string | null>(null);

</script>

<style scoped>
.fa-filter {
  max-width: 300px;
}
</style>
