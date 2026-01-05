<template>
  <v-dialog :model-value="modelValue" @update:model-value="emit('update:modelValue', $event)" @click:outside="emit('update:modelValue', false)" max-width="800">
    <v-card v-if="modelValue" :elevation="0">
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
              <v-list-item-title>{{ location.location.name }}</v-list-item-title>
              <v-list-item-subtitle>{{ location.location.address || location.location.description }}</v-list-item-subtitle>
            </v-list-item>
            <v-divider v-if="index < familyLocations.length - 1"></v-divider>
          </template>
        </v-list>
        <v-pagination v-if="totalItems > itemsPerPage" v-model="page" :length="Math.ceil(totalItems / itemsPerPage)"
          rounded="circle" class="mt-4"></v-pagination>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" @click="emit('update:modelValue', false)">{{ t('common.close') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useFamilyLocationSearch } from '@/composables/family-location/useFamilyLocationSearch';
import type { FamilyLocation } from '@/types';
import { computed } from 'vue'; // Import computed

interface LocationDialogProps {
  modelValue: boolean; // Controls dialog visibility
  familyId?: string | null; // FamilyId for filtering locations
}

const props = defineProps<LocationDialogProps>();
const emit = defineEmits(['update:modelValue', 'selectLocation']); // Emits for visibility and selection

const { t } = useI18n();

const currentFamilyId = computed(() => props.familyId ?? null); // Ensure it's string | null

const {
  familyLocations,
  totalItems,
  isLoading,
  searchQuery,
  page,
  itemsPerPage,
} = useFamilyLocationSearch(currentFamilyId); // Pass the reactive ref

const handleSelectLocation = (location: FamilyLocation) => {
  emit('selectLocation', location);
  emit('update:modelValue', false); // Close dialog after selection
};
</script>
