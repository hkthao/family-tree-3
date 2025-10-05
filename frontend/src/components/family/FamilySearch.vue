<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ $t('member.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small"  @click="expanded = !expanded">
        <v-icon>{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <!-- Search + Filter -->
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="searchQuery"
                :label="$t('family.management.searchLabel')"
                clearable
                prepend-inner-icon="mdi-magnify"
              />
            </v-col>
            <v-col cols="12" md="6">
              <v-select
                v-model="visibility"
                :items="visibilityItems"
                :label="$t('family.management.filterLabel')"
                density="compact"
              />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{ $t('member.search.apply') }}</v-btn>
          <v-btn @click="resetFilters">{{ $t('member.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyFilter } from '@/types';

const emit = defineEmits(['update:filters', 'create']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const searchQuery = ref('');
const visibility = ref<'All' | 'Private' | 'Public'>('All');

const visibilityItems = computed(() => [
  { title: t('family.management.visibility.all'), value: 'All' },
  { title: t('family.management.visibility.private'), value: 'Private' },
  { title: t('family.management.visibility.public'), value: 'Public' },
]);

const applyFilters = () => {
  emit('update:filters', {
    searchQuery: searchQuery.value,
    visibility: visibility.value === 'All' ? undefined : visibility.value,
  } as FamilyFilter);
};

const resetFilters = () => {
  searchQuery.value = '';
  visibility.value = 'All';
  // applyFilters(); // No need to call here, watch will handle it
};

// Watch for changes in filters and apply them automatically
watch([searchQuery, visibility], () => {
  applyFilters();
}, { immediate: true }); // immediate: true to apply filters on initial load

</script>
