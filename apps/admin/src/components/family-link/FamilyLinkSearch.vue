<template>
  <v-card :elevation="0" class="mb-4" data-testid="family-link-search">
    <v-card-title class="text-h6 d-flex align-center pa-0">
      {{ t('familyLink.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded" data-testid="family-link-search-expand-button">
        <v-tooltip :text="expanded ? t('common.collapse') : t('common.expand')">
          <template v-slot:activator="{ props }">
            <v-icon v-bind="props">{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </template>
        </v-tooltip>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text class="pa-0">
          <v-row>
            <v-col cols="12" md="6">
              <FamilyAutocomplete
                v-model="filters.otherFamilyId"
                :label="t('familyLink.search.otherFamily')"
                clearable
                data-testid="family-link-other-family-filter"
              />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="apply-filters-button">{{
            t('familyLink.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters" data-testid="reset-filters-button">{{ t('familyLink.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { type FamilyLinkFilter } from '@/types';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false);

const filters = ref<FamilyLinkFilter>({
  otherFamilyId: null,
});

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
    otherFamilyId: null,
  };
  emit('update:filters', filters.value);
};
</script>