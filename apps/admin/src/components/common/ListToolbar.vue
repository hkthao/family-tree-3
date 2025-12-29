<template>
  <v-toolbar flat>
    <v-toolbar-title>{{ title }}</v-toolbar-title>
    <v-spacer></v-spacer>
    <slot name="custom-buttons"></slot>

    <!-- New Add Link Button -->
    <v-btn
      v-if="addLinkButtonTooltip"
      color="primary"
      icon
      @click="emit('addLink')"
      data-testid="add-link-button"
      :aria-label="addLinkButtonTooltip"
    >
      <v-tooltip :text="addLinkButtonTooltip">
        <template v-slot:activator="{ props }">
          <v-icon v-bind="props">mdi-link-plus</v-icon>
        </template>
      </v-tooltip>
    </v-btn>

    <v-btn
      v-if="!hideCreateButton"
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
      v-if="!hideSearch"
      :model-value="searchQuery"
      @update:model-value="emit('update:search', $event)"
      :label="searchLabel"
      :placeholder="$t('common.search')"
      append-inner-icon="mdi-magnify"
      single-line
      hide-details
      clearable
      class="mr-2"
      data-test-id="list-toolbar-search-input"
    ></v-text-field>
  </v-toolbar>
</template>

<script setup lang="ts">


const { title, createButtonTooltip, createButtonTestId, hideCreateButton, searchQuery, searchLabel, hideSearch, addLinkButtonTooltip } = defineProps<{
  title: string;
  createButtonTooltip: string;
  createButtonTestId: string;
  hideCreateButton?: boolean;
  searchQuery?: string;
  searchLabel?: string;
  hideSearch?: boolean;
  addLinkButtonTooltip?: string; // New prop
}>();

const emit = defineEmits(['create', 'update:search', 'addLink']); // New emit
</script>

<style scoped></style>