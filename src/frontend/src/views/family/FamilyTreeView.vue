<template>
  <v-card data-testid="family-tree-view">
    <v-card-title>
      <div class="d-flex flex-row">
        <h1 class="text-h5 flex-grow-1">
          {{ t('family.tree.title') }}
        </h1>
        <family-auto-complete v-model="selectedFamilyId" class="tree-filter-input"
          :label="t('family.tree.filterByFamily')" clearable @update:modelValue="onFamilyChange" hideDetails />
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
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { TreeChart } from '@/components/family';

const { t } = useI18n();
const selectedFamilyId = ref<string | null>(null);
const onFamilyChange = (familyId: string | null) => {
  selectedFamilyId.value = familyId;
};
</script>
<style>
.tree-filter-input {
  margin-top: 3px;
  max-width: 320px;
}
</style>
