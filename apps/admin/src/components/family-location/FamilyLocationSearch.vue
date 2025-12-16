<template>
  <v-card class="mb-4 pa-4" elevation="0">
    <v-row align="center">
      <v-col cols="12" md="6">
        <v-text-field
          v-model="search"
          :label="t('common.search')"
          append-inner-icon="mdi-magnify"
          single-line
          hide-details
          clearable
          data-testid="family-location-search-input"
          @update:model-value="handleSearchInput"
        ></v-text-field>
      </v-col>
    </v-row>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useDebounceFn } from '@vueuse/core';

const emit = defineEmits(['update:search']);

const { t } = useI18n();
const search = ref('');

// Debounce the search input to avoid excessive updates
const debouncedSearch = useDebounceFn(() => {
  emit('update:search', search.value);
}, 500);

const handleSearchInput = (value: string | null) => {
  search.value = value || '';
  debouncedSearch();
};
</script>