<template>
  <v-container fluid class="pa-0">
    <v-card>
      <v-tabs v-model="tab" align-tabs="center" color="primary">
        <v-tab value="hierarchical">{{ t('familyTree.tab.hierarchical') }}</v-tab>
        <v-tab value="force-directed">{{ t('familyTree.tab.forceDirected') }}</v-tab>
      </v-tabs>

      <v-card-text>
        <v-window v-model="tab">
          <v-window-item value="hierarchical">
            <HierarchicalFamilyTree
              v-if="familyId"
              :family-id="familyId"
              :members="members"
              :relationships="relationships"
            />
            <v-alert v-else type="info" prominent>{{ t('familyTree.noDataMessage') }}</v-alert>
          </v-window-item>

          <v-window-item value="force-directed">
            <ForceDirectedFamilyTree
              v-if="familyId"
              :family-id="familyId"
              :members="members"
              :relationships="relationships"
            />
            <v-alert v-else type="info" prominent>{{ t('familyTree.noDataMessage') }}</v-alert>
          </v-window-item>
        </v-window>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import HierarchicalFamilyTree from '@/components/family/HierarchicalFamilyTree.vue';
import ForceDirectedFamilyTree from '@/components/family/ForceDirectedFamilyTree.vue';
import type { Member, Relationship } from '@/types'; // Assuming these types are defined globally or imported

interface WindowFamilyTreeData {
  familyId?: string;
  members?: Member[];
  relationships?: Relationship[];
}

const { t } = useI18n();

const tab = ref('hierarchical');
const familyId = ref<string | undefined>(undefined);
const members = ref<Member[]>([]);
const relationships = ref<Relationship[]>([]);

onMounted(() => {
  if (window.familyTreeData) {
    const data: WindowFamilyTreeData = window.familyTreeData;
    familyId.value = data.familyId;
    members.value = data.members || [];
    relationships.value = data.relationships || [];
  }
});
</script>

<style scoped>
/* Add any specific styles for this view here */
</style>
