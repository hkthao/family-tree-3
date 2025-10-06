<template>
  <v-card v-if="relationship" class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ relationship.sourceMember?.fullName }} - {{ relationship.targetMember?.fullName }} ({{ t(`relationship.type.${relationship.type.toLowerCase()}`) }})
      <v-spacer></v-spacer>
    </v-card-title>
    <v-card-text>
      <v-tabs v-model="selectedTab" class="mb-4">
        <v-tab value="general">{{ t('relationship.form.tab.general') }}</v-tab>
      </v-tabs>

      <v-window v-model="selectedTab">
        <v-window-item value="general">
          <div class="mt-4">
            <RelationshipForm :initial-relationship-data="relationship" :read-only="true"
              :title="t('relationship.detail.title')" />
          </div>
        </v-window-item>
      </v-window>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="gray" @click="closeView">
        {{ t('common.close') }}
      </v-btn>
      <v-btn color="primary" @click="navigateToEditRelationship(relationship.id)">
        {{ t('common.edit') }}
      </v-btn>
    </v-card-actions>
  </v-card>
  <v-alert v-else-if="!loading" type="info" class="mt-4" variant="tonal">
    {{ t('common.noData') }}
  </v-alert>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useRelationshipStore } from '@/stores/relationship.store';
import { RelationshipForm } from '@/components/relationships';
import type { Relationship } from '@/types';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const relationshipStore = useRelationshipStore();

const relationship = ref<Relationship | undefined>(undefined);
const loading = ref(false);
const selectedTab = ref('general');

const loadRelationship = async () => {
  loading.value = true;
  const relationshipId = route.params.id as string;
  if (relationshipId) {
    await relationshipStore.getById(relationshipId);
    relationship.value = relationshipStore.currentItem;
  }
  loading.value = false;
};

const navigateToEditRelationship = (id: string) => {
  router.push(`/relationships/edit/${id}`);
};

const closeView = () => {
  router.push('/relationships');
};

onMounted(() => {
  loadRelationship();
});

watch(
  () => route.params.id,
  (newId) => {
    if (newId) {
      loadRelationship();
    }
  },
);
</script>
