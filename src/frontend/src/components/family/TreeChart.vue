<template>
  <v-toolbar class="mb-1">
    <v-btn-toggle color="primary" class="ms-4" v-model="chartMode" mandatory>
      <v-btn value="hierarchical">
        <v-icon start>mdi-file-tree</v-icon>
        {{ t('family.tree.view.hierarchical') }}
      </v-btn>
      <v-btn value="force-directed">
        <v-icon start>mdi-share-variant</v-icon>
        {{ t('family.tree.view.network') }}
      </v-btn>
    </v-btn-toggle>
    <v-spacer></v-spacer>
    <v-tooltip :text="t('member.add')">
      <template v-slot:activator="{ props }">
        <v-btn icon v-bind="props" color="primary" @click="handleAddMember" class="mr-2">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </template>
    </v-tooltip>
    <v-text-field v-model="treeVisualizationStore.searchQuery" label="Tìm kiếm thành viên"
      prepend-inner-icon="mdi-magnify" single-line hide-details clearable class="mr-4"></v-text-field>
  </v-toolbar>
  <HierarchicalFamilyTree v-if="chartMode === 'hierarchical'" :family-id="props.familyId"
    @add-member="emit('add-member')" @edit-member="emit('edit-member', $event)"
    @delete-member="emit('delete-member', $event)" @add-father="emit('add-father', $event)"
    @add-mother="emit('add-mother', $event)" @add-child="emit('add-child', $event)" />
  <ForceDirectedFamilyTree v-else :family-id="props.familyId" />
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { HierarchicalFamilyTree, ForceDirectedFamilyTree } from '@/components/family';
import { useTreeVisualizationStore } from '@/stores/tree-visualization.store';

const props = defineProps({
  familyId: { type: String, default: null },
});
const emit = defineEmits([
  'add-member',
  'edit-member',
  'delete-member',
  'add-father',
  'add-mother',
  'add-child',
]);
const { t } = useI18n();
const chartMode = ref('hierarchical'); // Default view
const treeVisualizationStore = useTreeVisualizationStore();
const handleAddMember = () => {
  emit('add-member');
};
</script>
