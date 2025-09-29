<template>
  <v-row class="align-center mb-1" justify="space-between">
    <v-col cols="12" md="auto" class="text-md-right">
      <v-btn-toggle v-model="chartMode" mandatory variant="outlined">
        <v-btn size="small"  value="hierarchical">
          <v-icon start>mdi-file-tree</v-icon>
          {{ t('family.tree.view.hierarchical') }}
        </v-btn>
        <v-btn size="small"  value="force-directed">
          <v-icon start>mdi-share-variant</v-icon>
          {{ t('family.tree.view.network') }}
        </v-btn>
      </v-btn-toggle>
    </v-col>
  </v-row>
  <HierarchicalFamilyTree
    v-if="chartMode === 'hierarchical'"
    :family-id="props.familyId"
  />
  <ForceDirectedFamilyTree v-else :family-id="props.familyId" />
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import HierarchicalFamilyTree from '@/components/family/HierarchicalFamilyTree.vue';
import ForceDirectedFamilyTree from '@/components/family/ForceDirectedFamilyTree.vue';

const props = defineProps({
  familyId: { type: String, default: null },
});
const { t } = useI18n();
const chartMode = ref('hierarchical'); // Default view
</script>
