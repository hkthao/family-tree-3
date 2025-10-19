<template>
  <v-card flat>
    <v-card-text>
      <v-tabs v-model="currentTab" color="primary" align-tabs="start">
        <v-tab value="aiChat">{{ t('systemConfig.tabs.aiChat') }}</v-tab>
        <v-tab value="embedding">{{ t('systemConfig.tabs.embedding') }}</v-tab>
        <v-tab value="vectorStore">{{ t('systemConfig.tabs.vectorStore') }}</v-tab>
        <v-tab value="storage">{{ t('systemConfig.tabs.storage') }}</v-tab>
        <v-tab value="systemFixed">{{ t('systemConfig.tabs.systemFixed') }}</v-tab>
      </v-tabs>

      <v-window v-model="currentTab" class="mt-4">
        <v-window-item value="aiChat">
          <v-row class="mt-4">
            <template v-for="config in aiChatConfigs" :key="config.key">
              <v-col cols="6">
                <v-text-field v-model="config.value" :label="config.key" :hint="config.description" persistent-hint
                  :readonly="isConfigReadOnly(config.key)" variant="outlined" class="mb-4"></v-text-field>
              </v-col>
            </template>
          </v-row>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="resetChanges(aiChatConfigs)">{{ t('systemConfig.actions.cancel') }}</v-btn>
            <v-btn color="primary" @click="saveChanges(aiChatConfigs)" :loading="systemConfigStore.loading">{{
              t('systemConfig.actions.save') }}</v-btn>
          </v-card-actions>
        </v-window-item>

        <v-window-item value="embedding">
          <v-row class="mt-4">
            <template v-for="config in embeddingConfigs" :key="config.key">
              <v-col cols="6">
                <v-text-field v-model="config.value" :label="config.key" :hint="config.description" persistent-hint
                  :readonly="isConfigReadOnly(config.key)" variant="outlined" class="mb-4"></v-text-field>
              </v-col>
            </template>
          </v-row>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="resetChanges(embeddingConfigs)">{{ t('systemConfig.actions.cancel')
              }}</v-btn>
            <v-btn color="primary" @click="saveChanges(embeddingConfigs)" :loading="systemConfigStore.loading">{{
              t('systemConfig.actions.save') }}</v-btn>
          </v-card-actions>
        </v-window-item>

        <v-window-item value="vectorStore">
          <v-row>
            <template v-for="config in vectorStoreConfigs" :key="config.key">
              <v-col cols="6">
                <v-text-field v-model="config.value" :label="config.key" :hint="config.description" persistent-hint
                  :readonly="isConfigReadOnly(config.key)" variant="outlined" class="mb-4"></v-text-field>
              </v-col>
            </template>
          </v-row>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="resetChanges(vectorStoreConfigs)">{{ t('systemConfig.actions.cancel')
              }}</v-btn>
            <v-btn color="primary" @click="saveChanges(vectorStoreConfigs)" :loading="systemConfigStore.loading">{{
              t('systemConfig.actions.save') }}</v-btn>
          </v-card-actions>
        </v-window-item>

        <v-window-item value="storage">
          <v-row class="mt-4">
            <template v-for="config in storageConfigs" :key="config.key">
              <v-col cols="6">
                <v-text-field v-model="config.value" :label="config.key" :hint="config.description" persistent-hint
                  :readonly="isConfigReadOnly(config.key)" variant="outlined" class="mb-4"></v-text-field>
              </v-col>
            </template>
          </v-row>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="resetChanges(storageConfigs)">{{ t('systemConfig.actions.cancel')
              }}</v-btn>
            <v-btn color="primary" @click="saveChanges(storageConfigs)" :loading="systemConfigStore.loading">{{
              t('systemConfig.actions.save') }}</v-btn>
          </v-card-actions>
        </v-window-item>

        <v-window-item value="systemFixed">
          <v-row class="mt-4">
            <template v-for="config in systemFixedConfigs" :key="config.key">
              <v-col cols="6">
                <v-text-field v-model="config.value" :label="config.key" :hint="config.description" persistent-hint
                  :readonly="isConfigReadOnly(config.key)" variant="outlined" class="mb-4"></v-text-field>
              </v-col>
            </template>
          </v-row>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="resetChanges(systemFixedConfigs)">{{ t('systemConfig.actions.cancel')
              }}</v-btn>
            <v-btn color="primary" @click="saveChanges(systemFixedConfigs)" :loading="systemConfigStore.loading">{{
              t('systemConfig.actions.save') }}</v-btn>
          </v-card-actions>
        </v-window-item>
      </v-window>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useSystemConfigStore } from '@/stores/system-config.store';
import type { SystemConfig } from '@/types';

const { t } = useI18n();
const systemConfigStore = useSystemConfigStore();

const currentTab = ref('aiChat');

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

// Computed properties to filter configs by tab
const aiChatConfigs = computed(() =>
  editableConfigs.value.filter((config) => config.key.startsWith('AIChat'))
);
const embeddingConfigs = computed(() =>
  editableConfigs.value.filter((config) => config.key.startsWith('Embedding'))
);
const vectorStoreConfigs = computed(() =>
  editableConfigs.value.filter((config) => config.key.startsWith('VectorStore'))
);
const storageConfigs = computed(() =>
  editableConfigs.value.filter((config) => config.key.startsWith('Storage'))
);
const systemFixedConfigs = computed(() =>
  editableConfigs.value.filter((config) => isConfigReadOnly(config.key))
);

const saveChanges = async (configsToSave: SystemConfig[]) => {
  for (const config of configsToSave) {
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

const resetChanges = (configsToReset: SystemConfig[]) => {
  // Reset only the configs in the current tab to their original values
  configsToReset.forEach(config => {
    const original = originalConfigs.value.find(oc => oc.key === config.key);
    if (original) {
      config.value = original.value;
    }
  });
};
</script>

<style scoped>
/* Add any specific styles for this component here */
</style>