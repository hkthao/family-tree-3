<template>
  <v-card>
    <v-card-title class="text-h5">{{ t('systemConfig.title') }}</v-card-title>
    <v-card-text>
      <v-text-field v-model="searchQuery" :label="t('systemConfig.search.label')" prepend-inner-icon="mdi-magnify"
        clearable class="mb-4"></v-text-field>

      <v-row>
        <template v-for="config in filteredAndSortedConfigs" :key="config.key">
          <v-col cols="6">
            <v-text-field v-model="config.value" :label="config.key" :hint="config.description" persistent-hint
              class="mb-4" append-inner-icon="mdi-content-save" @click:append-inner="saveConfig(config)"
              variant="outlined"></v-text-field>
          </v-col>
        </template>
      </v-row>
    </v-card-text>
    <v-card-actions class="pa-4">
      <v-spacer></v-spacer>
      <v-btn color="secondary" @click="close">{{ t('systemConfig.actions.cancel') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useSystemConfigStore } from '@/stores/system-config.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { SystemConfig } from '@/types';

const { t } = useI18n();
const systemConfigStore = useSystemConfigStore();
const notificationStore = useNotificationStore();

const searchQuery = ref('');

onMounted(async () => {
  await systemConfigStore.fetchSystemConfigs();
});

// Computed property to filter and sort configs
const filteredAndSortedConfigs = computed(() => {
  let filtered = systemConfigStore.configs;
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

const saveConfig = async (config: SystemConfig) => {
  await systemConfigStore.updateSystemConfig(config.id, config);
  if (!systemConfigStore.error) {
    notificationStore.showSnackbar(
      t('systemConfig.messages.updateSuccess'),
      'success'
    );
  } else {
    notificationStore.showSnackbar(
      systemConfigStore.error || t('systemConfig.messages.updateError'),
      'error'
    );
  }
};

const close = () => {
};

</script>