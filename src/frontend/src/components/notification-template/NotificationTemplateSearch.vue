<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ $t('notificationTemplate.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded">
        <v-icon>{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="searchQuery"
                :label="$t('notificationTemplate.search.searchLabel')"
                clearable
                prepend-inner-icon="mdi-magnify"
              />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{ $t('common.apply') }}</v-btn>
          <v-btn @click="resetFilters">{{ $t('common.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const searchQuery = ref('');

const applyFilters = () => {
  emit('update:filters', { search: searchQuery.value });
};

const resetFilters = () => {
  searchQuery.value = '';
  applyFilters();
};

// Watch for changes in filters and apply them automatically
watch(searchQuery, () => {
  applyFilters();
});
</script>
