<template>
  <v-container fluid>
    <v-card>
      <v-card-title class="text-h5">{{ t('systemConfig.title') }}</v-card-title>
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
            <v-card flat>
              <v-card-text>
                <h3 class="text-h6 mb-4">{{ t('systemConfig.sections.aiChat') }}</h3>
                <!-- AI Chat Settings -->
                <template v-for="config in aiChatConfigs" :key="config.key">
                  <v-text-field
                    v-if="config.type === 'string'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-switch
                    v-else-if="config.type === 'boolean'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    color="primary"
                    inset
                    class="mb-4"
                  ></v-switch>
                  <v-text-field
                    v-else-if="config.type === 'integer'"
                    v-model.number="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    type="number"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-select
                    v-else-if="config.type === 'enum' && config.options"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    :items="config.options"
                    item-title="text"
                    item-value="value"
                    variant="outlined"
                    class="mb-4"
                  ></v-select>
                  <v-textarea
                    v-else-if="config.type === 'json'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    rows="5"
                    class="mb-4"
                  ></v-textarea>
                </template>
              </v-card-text>
            </v-card>
          </v-window-item>

          <v-window-item value="embedding">
            <v-card flat>
              <v-card-text>
                <h3 class="text-h6 mb-4">{{ t('systemConfig.sections.embedding') }}</h3>
                <!-- Embedding Settings -->
                <template v-for="config in embeddingConfigs" :key="config.key">
                  <v-text-field
                    v-if="config.type === 'string'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-switch
                    v-else-if="config.type === 'boolean'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    color="primary"
                    inset
                    class="mb-4"
                  ></v-switch>
                  <v-text-field
                    v-else-if="config.type === 'integer'"
                    v-model.number="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    type="number"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-select
                    v-else-if="config.type === 'enum' && config.options"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    :items="config.options"
                    item-title="text"
                    item-value="value"
                    variant="outlined"
                    class="mb-4"
                  ></v-select>
                  <v-textarea
                    v-else-if="config.type === 'json'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    rows="5"
                    class="mb-4"
                  ></v-textarea>
                </template>
              </v-card-text>
            </v-card>
          </v-window-item>

          <v-window-item value="vectorStore">
            <v-card flat>
              <v-card-text>
                <h3 class="text-h6 mb-4">{{ t('systemConfig.sections.vectorStore') }}</h3>
                <!-- Vector Store Settings -->
                <template v-for="config in vectorStoreConfigs" :key="config.key">
                  <v-text-field
                    v-if="config.type === 'string'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-switch
                    v-else-if="config.type === 'boolean'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    color="primary"
                    inset
                    class="mb-4"
                  ></v-switch>
                  <v-text-field
                    v-else-if="config.type === 'integer'"
                    v-model.number="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    type="number"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-select
                    v-else-if="config.type === 'enum' && config.options"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    :items="config.options"
                    item-title="text"
                    item-value="value"
                    variant="outlined"
                    class="mb-4"
                  ></v-select>
                  <v-textarea
                    v-else-if="config.type === 'json'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    rows="5"
                    class="mb-4"
                  ></v-textarea>
                </template>
              </v-card-text>
            </v-card>
          </v-window-item>

          <v-window-item value="storage">
            <v-card flat>
              <v-card-text>
                <h3 class="text-h6 mb-4">{{ t('systemConfig.sections.storage') }}</h3>
                <!-- Storage Settings -->
                <template v-for="config in storageConfigs" :key="config.key">
                  <v-text-field
                    v-if="config.type === 'string'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-switch
                    v-else-if="config.type === 'boolean'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    color="primary"
                    inset
                    class="mb-4"
                  ></v-switch>
                  <v-text-field
                    v-else-if="config.type === 'integer'"
                    v-model.number="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    type="number"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-select
                    v-else-if="config.type === 'enum' && config.options"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    :items="config.options"
                    item-title="text"
                    item-value="value"
                    variant="outlined"
                    class="mb-4"
                  ></v-select>
                  <v-textarea
                    v-else-if="config.type === 'json'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    rows="5"
                    class="mb-4"
                  ></v-textarea>
                </template>
              </v-card-text>
            </v-card>
          </v-window-item>

          <v-window-item value="systemFixed">
            <v-card flat>
              <v-card-text>
                <h3 class="text-h6 mb-4">{{ t('systemConfig.sections.systemFixed') }}</h3>
                <!-- System/Fixed Settings -->
                <template v-for="config in systemFixedConfigs" :key="config.key">
                  <v-text-field
                    v-if="config.type === 'string'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-switch
                    v-else-if="config.type === 'boolean'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    color="primary"
                    inset
                    class="mb-4"
                  ></v-switch>
                  <v-text-field
                    v-else-if="config.type === 'integer'"
                    v-model.number="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    type="number"
                    variant="outlined"
                    class="mb-4"
                  ></v-text-field>
                  <v-select
                    v-else-if="config.type === 'enum' && config.options"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    :items="config.options"
                    item-title="text"
                    item-value="value"
                    variant="outlined"
                    class="mb-4"
                  ></v-select>
                  <v-textarea
                    v-else-if="config.type === 'json'"
                    v-model="config.value"
                    :label="config.description || config.key"
                    :readonly="config.isReadOnly"
                    variant="outlined"
                    rows="5"
                    class="mb-4"
                  ></v-textarea>
                </template>
              </v-card-text>
            </v-card>
          </v-window-item>
        </v-window>
      </v-card-text>
      <v-card-actions class="pa-4">
        <v-spacer></v-spacer>
        <v-btn color="secondary" @click="resetChanges">{{ t('systemConfig.actions.cancel') }}</v-btn>
        <v-btn color="primary" @click="saveChanges" :loading="systemConfigStore.loading">{{ t('systemConfig.actions.save') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useSystemConfigStore } from '@/stores/system-config.store';
import { SystemConfig } from '@/types';

const { t } = useI18n();
const systemConfigStore = useSystemConfigStore();

const currentTab = ref('aiChat');

// Local state to hold editable configurations
const editableConfigs = ref<SystemConfig[]>([]);

onMounted(async () => {
  await systemConfigStore.fetchSystemConfigs();
  // Initialize editableConfigs with a deep copy of the store's configs
  editableConfigs.value = JSON.parse(JSON.stringify(systemConfigStore.configs));
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
  editableConfigs.value.filter((config) => config.isReadOnly)
);

const saveChanges = async () => {
  for (const config of editableConfigs.value) {
    // Only update if the value has changed and it's not read-only
    const originalConfig = systemConfigStore.configs.find(c => c.key === config.key);
    if (originalConfig && originalConfig.value !== config.value && !config.isReadOnly) {
      await systemConfigStore.updateSystemConfig(config.key, config.value);
    }
  }
  // Re-fetch to ensure local state is in sync after all updates
  await systemConfigStore.fetchSystemConfigs();
};

const resetChanges = () => {
  // Reset editableConfigs to the last fetched state from the store
  editableConfigs.value = JSON.parse(JSON.stringify(systemConfigStore.configs));
};
</script>

<style scoped>
/* Add any specific styles for this component here */
</style>