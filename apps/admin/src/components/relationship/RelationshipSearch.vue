<template>
  <v-card class="mb-4" data-testid="relationship-search">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('relationship.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded" data-testid="relationship-search-expand-button">
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
              <member-auto-complete
                v-model="filters.sourceMemberId"
                :label="t('relationship.search.sourceMember')"
                clearable
                data-testid="relationship-search-source-member-autocomplete"
              />
            </v-col>
            <v-col cols="12" md="4">
              <member-auto-complete
                v-model="filters.targetMemberId"
                :label="t('relationship.search.targetMember')"
                clearable
                data-testid="relationship-search-target-member-autocomplete"
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-select
                v-model="filters.type"
                :items="relationshipTypes"
                :label="t('relationship.search.type')"
                clearable
                data-testid="relationship-search-type-select"
              ></v-select>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="relationship-search-apply-button">{{
            t('relationship.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters" data-testid="relationship-search-reset-button">{{ t('relationship.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { RelationshipFilter } from '@/types';
import { RELATIONSHIP_TYPE_OPTIONS } from '@/constants/relationshipTypes'; 

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false);

const filters = ref<RelationshipFilter>({
  sourceMemberId: null,
  targetMemberId: null,
  type: null,
});

const relationshipTypes = RELATIONSHIP_TYPE_OPTIONS; // Used RELATIONSHIP_TYPE_OPTIONS

watch(
  filters,
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
    sourceMemberId: null,
    targetMemberId: null,
    type: null,
  };
  emit('update:filters', filters.value);
};

// Initial apply of filters on component mount
applyFilters();
</script>
