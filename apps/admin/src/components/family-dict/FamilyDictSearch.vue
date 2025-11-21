<template>
  <v-card class="mb-4" data-testid="family-dict-search">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('familyDict.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded"
        data-testid="family-dict-search-expand-button">
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
            <v-col cols="12" md="6">
              <v-select v-model="filters.lineage" :items="familyDictLineages" :label="t('familyDict.search.lineage')"
                clearable data-testid="family-dict-lineage-filter"></v-select>
            </v-col>
            <v-col cols="12" md="6">
              <v-select v-model="filters.region" :items="regions" :label="t('familyDict.search.region')" clearable
                data-testid="family-dict-region-filter"></v-select>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="apply-filters-button">{{
            t('familyDict.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters" data-testid="reset-filters-button">{{ t('familyDict.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDictFilter } from '@/types';
import { FamilyDictLineage } from '@/types';

const emit = defineEmits(['update:filters']);
const { t } = useI18n();
const expanded = ref(false);

const filters = ref<FamilyDictFilter>({
  searchQuery: '',
  lineage: undefined,
  region: undefined,
});

const familyDictLineages = computed(() => [
  { title: t('familyDict.lineage.noi'), value: FamilyDictLineage.Noi },
  { title: t('familyDict.lineage.ngoai'), value: FamilyDictLineage.Ngoai },
  { title: t('familyDict.lineage.noiNgoai'), value: FamilyDictLineage.NoiNgoai },
  { title: t('familyDict.lineage.other'), value: FamilyDictLineage.Other },
]);

const regions = computed(() => [
  { title: t('familyDict.form.namesByRegion.north'), value: 'north' },
  { title: t('familyDict.form.namesByRegion.central'), value: 'central' },
  { title: t('familyDict.form.namesByRegion.south'), value: 'south' },
]);

watch(
  filters.value,
  () => {
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
    lineage: undefined,
    region: undefined,
  };
  emit('update:filters', filters.value);
};
</script>
