<template>
  <v-container fluid>
    <v-card>
      <v-card-title>
        <div class="d-flex flex-row">
          <h1 class="text-h5 flex-grow-1">
            {{ t('family.tree.title') }}
          </h1>
          <FamilyAutocomplete
            v-model="selectedFamilyId"
            class="tree-filter-input"
            :label="t('family.tree.filterByFamily')"
            clearable
            @update:modelValue="onFamilyChange"
          />
        </div>
      </v-card-title>

      <v-card-text>
        <div v-if="!selectedFamilyId" class="text-center pa-8">
          <p>{{ t('event.messages.selectFamily') }}</p>
        </div>
        <template v-else>
          <TreeChart :family-id="selectedFamilyId" />
        </template>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import TreeChart from '@/components/family/TreeChart.vue';

const { t } = useI18n();
const selectedFamilyId = ref<string | null>(null);
const onFamilyChange = (familyId: string | null) => {
  selectedFamilyId.value = familyId;
};
</script>
<style>
.tree-filter-input {
  max-width: 320px;
}
</style>
