<template>
  <v-container fluid class="public-family-tree-viewer fill-height">
    <v-row v-if="loading" align="center" justify="center">
      <v-col cols="12" class="text-center">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">{{ t('common.loading') }}</p>
      </v-col>
    </v-row>

    <v-row v-else-if="error" align="center" justify="center">
      <v-col cols="12" class="text-center">
        <v-alert type="error" prominent>{{ error }}</v-alert>
      </v-col>
    </v-row>

    <v-row v-else-if="!family || members.length === 0" align="center" justify="center">
      <v-col cols="12" class="text-center">
        <v-alert type="info" prominent>{{ t('familyTree.noMembersMessage') }}</v-alert>
      </v-col>
    </v-row>

    <v-row v-else class="fill-height">
      <v-col cols="12" class="fill-height">
        <div class="hierarchical-tree-container">
          <div ref="chartContainer" class="f3 flex-grow-1" data-testid="public-family-tree-canvas"></div>
          <div class="legend">
            <div class="legend-item">
              <span class="legend-color-box legend-male"></span>
              <span>{{ t('member.gender.male') }}</span>
            </div>
            <div class="legend-item">
              <span class="legend-color-box legend-female"></span>
              <span>{{ t('member.gender.female') }}</span>
            </div>
          </div>
        </div>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, reactive } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { usePublicFamilyStore } from '@/stores/publicFamily.store';
import { usePublicMemberStore } from '@/stores/publicMember.store';
import { useHierarchicalTreeChart } from '@/composables/useHierarchicalTreeChart';
import type { Family, Member, Relationship } from '@/types';
import { RelationshipType } from '@/types';

const route = useRoute();
const { t } = useI18n();
const publicFamilyStore = usePublicFamilyStore();
const publicMemberStore = usePublicMemberStore();

const family = ref<Family | null>(null);
const members = ref<Member[]>([]);
const relationships = ref<Relationship[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);

const currentFamilyId = ref<string | null>(null);
const currentRootId = ref<string | null>(null);

// Dummy emit function for useHierarchicalTreeChart, as we don't need to emit events in public view
const dummyEmit = (event: string, ...args: any[]) => {
  console.log(`PublicFamilyTreeViewer: Emitted event ${event} with args:`, args);
};

// Reactive props object for useHierarchicalTreeChart
const chartProps = reactive({
  familyId: currentFamilyId,
  members: members,
  relationships: relationships,
  rootId: currentRootId,
});

const { chartContainer } = useHierarchicalTreeChart(
  chartProps,
  dummyEmit,
  t
);

const fetchData = async () => {
  loading.value = true;
  error.value = null;
  try {
    const routeFamilyId = route.params.familyId as string;
    const routeRootId = route.params.rootId as string | undefined;

    if (!routeFamilyId) {
      error.value = t('familyTree.errors.familyIdMissing');
      return;
    }

    currentFamilyId.value = routeFamilyId;
    currentRootId.value = routeRootId || null;

    // Fetch family details
    const fetchedFamily = await publicFamilyStore.getPublicFamilyById(routeFamilyId);
    if (!fetchedFamily) {
      error.value = t('familyTree.errors.familyNotFound');
      return;
    }
    family.value = fetchedFamily;

    // Fetch members
    const fetchedMembers = await publicMemberStore.getPublicMembersByFamilyId(routeFamilyId);
    if (!fetchedMembers) {
      error.value = t('familyTree.errors.membersNotFound');
      return;
    }
    members.value = fetchedMembers;

    // Infer relationships from fetched members
    const inferredRelationships: Relationship[] = [];
    members.value.forEach(member => {
      if (member.fatherId) {
        inferredRelationships.push({
          id: `rel-${member.fatherId}-${member.id}`,
          familyId: member.familyId,
          sourceMemberId: member.fatherId,
          targetMemberId: member.id,
          type: RelationshipType.Father,
          order: null,
        });
      }
      if (member.motherId) {
        inferredRelationships.push({
          id: `rel-${member.motherId}-${member.id}`,
          familyId: member.familyId,
          sourceMemberId: member.motherId,
          targetMemberId: member.id,
          type: RelationshipType.Mother,
          order: null,
        });
      }
      if (member.husbandId) {
        inferredRelationships.push({
          id: `rel-${member.id}-${member.husbandId}`, // Member is wife of husbandId
          familyId: member.familyId,
          sourceMemberId: member.id,
          targetMemberId: member.husbandId,
          type: RelationshipType.Wife,
          order: null,
        });
      }
      if (member.wifeId) {
        inferredRelationships.push({
          id: `rel-${member.id}-${member.wifeId}`, // Member is husband of wifeId
          familyId: member.familyId,
          sourceMemberId: member.id,
          targetMemberId: member.wifeId,
          type: RelationshipType.Husband,
          order: null,
        });
      }
    });
    relationships.value = inferredRelationships;

  } catch (err) {
    console.error('Error fetching public family tree data:', err);
    error.value = t('familyTree.errors.fetchData');
  } finally {
    loading.value = false;
  }
};

onMounted(fetchData);

watch(
  () => [route.params.familyId, route.params.rootId],
  () => {
    fetchData();
  },
  { deep: true }
);

// No need for an extra watch here, as useHierarchicalTreeChart already watches its props.
// The reactive `chartProps` object ensures reactivity.

</script>

<style scoped>
@import './PublicFamilyTreeViewer.vue.css';

.public-family-tree-viewer {
  background-color: rgb(var(--v-theme-background));
}
</style>