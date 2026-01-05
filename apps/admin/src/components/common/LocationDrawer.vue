<template>
  <BaseCrudDrawer :class="cssClass" :model-value="props.modelValue" @update:model-value="emit('update:modelValue', $event)"
    @close="emit('update:modelValue', false)" :title="t('familyLocation.list.title')" data-testid="location-drawer">
    <v-tabs class="ma-4" v-model="tab" align-tabs="start" color="primary">
      <v-tab value="search">{{ t('common.searchAndSelect') }}</v-tab>
      <v-tab value="create" :disabled="!canCreateLocation">{{ t('common.createNew') }}</v-tab>
    </v-tabs>
    <v-window v-model="tab">
      <v-window-item class="pa-4" value="search">
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
              <v-list-item-subtitle>{{ location.location.address || location.location.description
              }}</v-list-item-subtitle>
            </v-list-item>
            <v-divider v-if="index < familyLocations.length - 1"></v-divider>
          </template>
        </v-list>
        <v-pagination v-if="totalItems > itemsPerPage" v-model="page" :length="Math.ceil(totalItems / itemsPerPage)"
          rounded="circle" class="mt-4"></v-pagination>
      </v-window-item>
      <v-window-item value="create" :disabled="!canCreateLocation">
        <FamilyLocationAddView v-if="canCreateLocation" :family-id="familyId as string" @saved="handleLocationAdded"
          @close="handleCloseFamilyLocationAddView" />
        <v-alert v-else type="info" class="mt-4">{{ t('familyLocation.messages.noFamilyIdForCreation') }}</v-alert>
      </v-window-item>
    </v-window>
  </BaseCrudDrawer>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'; // Import watch
import { useI18n } from 'vue-i18n';
import { useFamilyLocationSearch } from '@/composables/family-location/useFamilyLocationSearch';
import type { FamilyLocation } from '@/types';
import FamilyLocationAddView from '@/views/family-location/FamilyLocationAddView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';

interface LocationDrawerProps {
  modelValue: boolean; // Controls dialog visibility
  familyId?: string | null; // Optional FamilyId for filtering locations
  cssClass?: string | null; // Optional CSS class for the drawer
}

const props = defineProps<LocationDrawerProps>();
const emit = defineEmits(['update:modelValue', 'selectLocation']); // Emits for visibility and selection

const { t } = useI18n();

const tab = ref<string | null>(null); // Controls the active tab

const canCreateLocation = computed(() => {
  return typeof props.familyId === 'string' && props.familyId.length > 0;
});

// Watch for changes in canCreateLocation and switch tab if necessary
watch(canCreateLocation, (newVal) => {
  if (!newVal && tab.value === 'create') {
    tab.value = 'search'; // Switch to search tab if create is no longer allowed
  }
});

const currentFamilyId = computed(() => props.familyId ?? null); // Re-introduce computed property

const {
  familyLocations,
  totalItems,
  isLoading,
  searchQuery,
  page,
  itemsPerPage,
  refetch, // Inject refetch
} = useFamilyLocationSearch(currentFamilyId); // Use currentFamilyId

const handleLocationAdded = () => {
  tab.value = 'search'; // Switch to search tab
  refetch(); // Reload data
  // emit('update:modelValue', false); // Do not close the drawer
};

const handleCloseFamilyLocationAddView = () => {
  emit('update:modelValue', false); // Close the dialog when FamilyLocationAddView emits close
};

const handleSelectLocation = (location: FamilyLocation) => {
  emit('selectLocation', location);
  emit('update:modelValue', false); // Close dialog after selection
};
</script>
