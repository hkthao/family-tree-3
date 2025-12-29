<template>
  <v-dialog v-model="locationDrawerStore.drawer" @click:outside="locationDrawerStore.closeDrawer" max-width="800">
    <v-card v-if="locationDrawerStore.drawer" :elevation="0">
      <v-card-title class="text-h5 text-uppercase text-center">
        {{ t('familyLocation.list.title') }}
      </v-card-title>
      <v-card-text>
        <v-text-field v-model="searchQuery" :label="t('common.search')" append-inner-icon="mdi-magnify" single-line
          hide-details class="mb-4"></v-text-field>

        <div v-if="isLoading" class="d-flex justify-center align-center py-4">
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
          {{ t('common.loading') }}
        </div>
        <div v-else-if="familyLocations.length === 0" class="text-center py-4">
          {{ t('familyLocation.list.noData') }}
        </div>
        <v-list v-else>
          <template v-for="(location, index) in familyLocations" :key="location.id">
            <v-list-item @click="handleSelectLocation(location)" link prepend-icon="mdi-map-marker">
              <v-list-item-title>{{ location.name }}</v-list-item-title>
              <v-list-item-subtitle>{{ location.address || location.description }}</v-list-item-subtitle>
            </v-list-item>
            <v-divider v-if="index < familyLocations.length - 1"></v-divider>
          </template>
        </v-list>
        <v-pagination v-if="totalItems > itemsPerPage" v-model="page" :length="Math.ceil(totalItems / itemsPerPage)"
          rounded="circle" class="mt-4"></v-pagination>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" @click="locationDrawerStore.closeDrawer">{{ t('common.close') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
// BaseCrudDrawer is no longer needed
// import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; 
import { useLocationDrawerStore } from '@/stores/locationDrawer.store';
import { useFamilyLocationSearch } from '@/composables/family-location/useFamilyLocationSearch';
import type { FamilyLocation } from '@/types';
import { toRef } from 'vue'; // Import toRef

const { t } = useI18n();
const locationDrawerStore = useLocationDrawerStore();
const familyIdRef = toRef(locationDrawerStore, 'initialFamilyId'); // Make it a reactive ref

const {
  familyLocations,
  totalItems,
  isLoading,
  searchQuery,
  page,
  itemsPerPage,
} = useFamilyLocationSearch(familyIdRef); // Pass the reactive ref

const handleSelectLocation = (location: FamilyLocation) => {
  locationDrawerStore.confirmSelection(location);
};
</script>
