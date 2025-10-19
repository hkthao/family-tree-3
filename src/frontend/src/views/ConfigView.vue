<template>
  <v-card>
    <v-card-title class="text-h5">{{ t('systemConfig.title') }}</v-card-title>
    <v-card-text>
      <v-text-field v-model="searchQuery" :label="t('systemConfig.search.label')" prepend-inner-icon="mdi-magnify"
        variant="outlined" clearable class="mb-4"></v-text-field>

      <v-row>
        <template v-for="config in filteredAndSortedConfigs" :key="config.key">
          <v-col cols="6">
            <v-text-field v-model="config.value" :label="config.key" :hint="config.description" persistent-hint
              :readonly="isConfigReadOnly(config.key)" variant="outlined" class="mb-4"></v-text-field>
          </v-col>
        </template>
      </v-row>
    </v-card-text>
    <v-card-actions class="pa-4">
      <v-spacer></v-spacer>
      <v-btn color="secondary" @click="resetChanges">{{ t('systemConfig.actions.cancel') }}</v-btn>
      <v-btn color="primary" @click="saveChanges" :loading="systemConfigStore.loading">{{ t('systemConfig.actions.save')
        }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useSystemConfigStore } from '@/stores/system-config.store';
import type { SystemConfig } from '@/types';

const { t } = useI18n();
const systemConfigStore = useSystemConfigStore();

const searchQuery = ref('');

// Define keys that should always be read-only
const READ_ONLY_KEYS = [
  'ConnectionStrings:DefaultConnection',
  'Jwt:Key',
  'Jwt:Issuer',
  'Jwt:Audience',
  'Cors:Origins',
  // Add other sensitive or fixed keys here
];

const isConfigReadOnly = (key: string): boolean => {
  return READ_ONLY_KEYS.includes(key);
};

// Local state to hold editable configurations
const editableConfigs = ref<SystemConfig[]>([]);
// Store original configurations for reset functionality
const originalConfigs = ref<SystemConfig[]>([]);

onMounted(async () => {
  await systemConfigStore.fetchSystemConfigs();
  // Initialize editableConfigs and originalConfigs with a deep copy of the store's configs
  editableConfigs.value = JSON.parse(JSON.stringify(systemConfigStore.configs));
  originalConfigs.value = JSON.parse(JSON.stringify(systemConfigStore.configs));
});

// Computed property to filter and sort configs
const filteredAndSortedConfigs = computed(() => {
  let filtered = editableConfigs.value;

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    filtered = filtered.filter(
      (config) =>
        config.key.toLowerCase().includes(query) ||
        config.description?.toLowerCase().includes(query) ||
        config.value?.toString().toLowerCase().includes(query)
    );
  }

  // Sort by key (name)
  return filtered.sort((a, b) => a.key.localeCompare(b.key));
});

const saveChanges = async () => {
  for (const config of editableConfigs.value) {
    // Only update if the value has changed and it's not read-only
    const originalConfig = originalConfigs.value.find(c => c.key === config.key);
    if (originalConfig && originalConfig.value !== config.value && !isConfigReadOnly(config.key)) {
      await systemConfigStore.updateSystemConfig(config.key, config.value);
    }
  }
  // After saving, re-fetch all configs to ensure consistency and update originalConfigs
  await systemConfigStore.fetchSystemConfigs();
  originalConfigs.value = JSON.parse(JSON.stringify(systemConfigStore.configs));
  editableConfigs.value = JSON.parse(JSON.stringify(systemConfigStore.configs));
};

const resetChanges = () => {
  // Reset editableConfigs to the last fetched state from the store
  editableConfigs.value = JSON.parse(JSON.stringify(systemConfigStore.configs));
};
</script>

<style scoped>
/* Add any specific styles for this component here */
</style>