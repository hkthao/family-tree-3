<template>
  <v-card class="mb-5" elevation="2">
    <v-card-title class="d-flex align-center">
      <v-icon icon="mdi-magnify" class="mr-2"></v-icon>
      {{ t('familyMedia.search.title') }}
      <v-spacer></v-spacer>
      <v-btn
        icon
        size="small"
        variant="text"
        @click="showFilters = !showFilters"
      >
        <v-icon>{{ showFilters ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="showFilters">
        <v-card-text>
          <v-row dense>
            <v-col cols="12" md="6" lg="4">
              <v-text-field
                v-model="filters.searchQuery"
                :label="t('familyMedia.search.searchQueryLabel')"
                density="compact"
                hide-details
                clearable
                @input="debouncedApplyFilters"
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
          <v-btn
            color="grey-darken-1"
            variant="text"
            @click="clearFilters"
          >
            {{ t('common.clearFilters') }}
          </v-btn>
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
import { debounce } from 'lodash'; // Assuming lodash is available for debouncing

const emit = defineEmits(['update:filters']);
const { t } = useI18n();

const showFilters = ref(false);

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

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const debouncedApplyFilters = debounce(applyFilters, 300);

const clearFilters = () => {
  filters.value = {
    searchQuery: undefined,
    mediaType: undefined,
  };
  emit('update:filters', filters.value); // Emit updated filters after clearing
};

// Removed the watch on filters to rely solely on debouncedApplyFilters or direct calls
// watch(filters, () => {
//   applyFilters();
// }, { deep: true });
</script>
