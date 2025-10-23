<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('member.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded">
        <v-tooltip :text="expanded ? t('common.collapse') : t('common.expand')">
          <template v-slot:activator="{ props }">
            <v-icon v-bind="props">{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </template>
        </v-tooltip>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field v-model="filters.searchQuery" :label="t('member.search.search')" clearable
                prepend-inner-icon="mdi-magnify"></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <GenderSelect v-model="filters.gender" :label="t('member.search.gender')" clearable />
            </v-col>
            <v-col cols="12" md="4">
              <FamilyAutocomplete v-model="filters.familyId" :label="t('member.search.family')" clearable />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{
            t('member.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters">{{ t('member.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberFilter } from '@/types';
import { GenderSelect, FamilyAutocomplete } from '@/components/common';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const filters = ref<MemberFilter>({
  searchQuery: '',
  gender: undefined,
  familyId: null,
});

watch(
  filters.value,
  () => {
    // Debounce or apply immediately based on preference
    applyFilters();
  },
  { deep: true },
);

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const resetFilters = () => {
  filters.value = {
    searchQuery: '',
    gender: undefined,
    familyId: null,
  };
  emit('update:filters', filters.value);
};
</script>
