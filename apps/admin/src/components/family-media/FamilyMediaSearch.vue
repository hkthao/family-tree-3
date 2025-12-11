<template>
  <v-card class="mb-4" elevation="0">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('familyMedia.search.title') }}
      <v-spacer></v-spacer>
      <v-btn
        variant="text"
        icon
        size="small"
        @click="expanded = !expanded"
        data-testid="family-media-search-expand-button"
      >
        <v-tooltip :text="expanded ? t('common.collapse') : t('common.expand')">
          <template v-slot:activator="{ props }">
            <v-icon v-bind="props">{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </template>
        </v-tooltip>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row dense>
            <v-col cols="12" md="6" lg="4">
              <v-text-field
                v-model="filters.searchQuery"
                :label="t('familyMedia.search.searchQueryLabel')"
                density="compact"
                hide-details
                clearable
                @update:model-value="applyFilters"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" lg="4">
              <v-select
                v-model="filters.mediaType"
                :items="mediaTypes"
                :label="t('familyMedia.search.mediaTypeLabel')"
                density="compact"
                hide-details
                clearable
                @update:model-value="applyFilters"
              ></v-select>
            </v-col>
            <!-- Add more filter options as needed (e.g., refType, refId if filtering for specific entities) -->
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="apply-filters-button">{{
            t('familyMedia.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters" data-testid="reset-filters-button">{{ t('common.resetFilters') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyMediaFilter } from '@/types';
import { MediaType } from '@/types/enums'; // Assuming enums are exported from here

const emit = defineEmits(['update:filters']);
const { t } = useI18n();

const expanded = ref(false);

const filters = ref<FamilyMediaFilter>({
  searchQuery: undefined,
  mediaType: undefined,
});

const mediaTypes = computed(() => {
  return Object.keys(MediaType)
    .filter(key => isNaN(Number(key))) // Filter out numeric keys from enum
    .map(key => ({
      title: t(`common.mediaType.${key}`),
      value: MediaType[key as keyof typeof MediaType],
    }));
});

watch(
  filters.value,
  () => {
    applyFilters();
  },
  { deep: true },
);

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const resetFilters = () => {
  filters.value = {
    searchQuery: undefined,
    mediaType: undefined,
  };
  emit('update:filters', filters.value); // Emit updated filters after resetting
};

// Removed the watch on filters to rely solely on debouncedApplyFilters or direct calls
// watch(filters, () => {
//   applyFilters();
// }, { deep: true });
</script>
