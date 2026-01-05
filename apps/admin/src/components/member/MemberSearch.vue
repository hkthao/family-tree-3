<template>
  <v-card :elevation="0" class="mb-4" data-testid="member-search">
    <v-card-title class="text-h6 d-flex align-center pa-0">
      {{ t('member.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded" data-testid="member-search-expand-button">
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
            <v-col cols="12" md="4">
              <GenderSelect v-model="filters.gender" :label="t('member.search.gender')" clearable
                data-testid="member-gender-filter" />
            </v-col>
            <v-col cols="12" md="4">
              <MemberAutocomplete
                v-model="filters.fatherId"
                :label="t('member.search.father')"
                clearable
                data-testid="member-father-filter"
              />
            </v-col>
            <v-col cols="12" md="4">
              <MemberAutocomplete
                v-model="filters.motherId"
                :label="t('member.search.mother')"
                clearable
                data-testid="member-mother-filter"
              />
            </v-col>
            <v-col cols="12" md="4">
              <MemberAutocomplete
                v-model="filters.husbandId"
                :label="t('member.search.husband')"
                clearable
                data-testid="member-husband-filter"
              />
            </v-col>
            <v-col cols="12" md="4">
              <MemberAutocomplete
                v-model="filters.wifeId"
                :label="t('member.search.wife')"
                clearable
                data-testid="member-wife-filter"
              />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="apply-filters-button">{{
            t('member.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters" data-testid="reset-filters-button">{{ t('member.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberFilter } from '@/types';
import { GenderSelect } from '@/components/common';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
const emit = defineEmits(['update:filters']);
const { t } = useI18n();
const expanded = ref(false); // Default to collapsed
const filters = ref<MemberFilter>({
  gender: undefined,
  fatherId: null,
  motherId: null,
  husbandId: null,
  wifeId: null,
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
    gender: undefined,
    fatherId: null,
    motherId: null,
    husbandId: null,
    wifeId: null,
  };
  emit('update:filters', filters.value);
};
</script>
