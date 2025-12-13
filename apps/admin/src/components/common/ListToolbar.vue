<template>
  <v-toolbar flat>
    <v-toolbar-title>{{ title }}</v-toolbar-title>
    <v-spacer></v-spacer>

    <v-btn
      color="primary"
      icon
      @click="emit('create')"
      :data-testid="createButtonTestId"
      :aria-label="createButtonTooltip"
    >
      <v-tooltip :text="createButtonTooltip">
        <template v-slot:activator="{ props }">
          <v-icon v-bind="props">mdi-plus</v-icon>
        </template>
      </v-tooltip>
    </v-btn>
    <v-text-field
      v-model="internalSearch"
      :label="searchPlaceholder"
      :data-test-id="searchInputTestId"
      append-inner-icon="mdi-magnify"
      single-line
      hide-details
      clearable
      class="mr-2"
    ></v-text-field>
  </v-toolbar>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps<{
  search: string;
  title: string;
  createButtonTooltip: string;
  searchPlaceholder: string;
  createButtonTestId: string;
  searchInputTestId: string;
}>();

const emit = defineEmits(['update:search', 'create']);


const internalSearch = ref(props.search);

watch(
  () => internalSearch.value,
  (newValue) => {
    emit('update:search', newValue ?? '');
  },
);

watch(
  () => props.search,
  (newSearch) => {
    if (newSearch !== internalSearch.value) {
      internalSearch.value = newSearch;
    }
  },
);
</script>

<style scoped></style>
