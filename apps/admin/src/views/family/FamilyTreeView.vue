<template>
  <v-card data-testid="family-tree-view">
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
import { TreeChart } from '@/components/family';
const { t } = useI18n();
const selectedFamilyId = ref<string | null>(null);
</script>