<template>
  <v-container fluid>
    <v-card v-if="family" class="mb-4">
      <v-card-title class="text-h6 d-flex align-center">
        {{ family.name }}
        <v-spacer></v-spacer>
      </v-card-title>
      <v-card-text>
        <v-tabs v-model="selectedTab" class="mb-4">
          <v-tab value="general">{{ t('member.form.tab.general') }}</v-tab>
          <v-tab value="timeline">{{ t('member.form.tab.timeline') }}</v-tab>
        </v-tabs>

        <v-window v-model="selectedTab">
          <v-window-item value="general">
            <FamilyForm
              :initial-family-data="family"
              :read-only="true"
              :title="t('family.detail.title')"
            />
          </v-window-item>

          <v-window-item value="timeline">
            <EventTimeline
              :family-id="family.id"
              :read-only="readOnly"
            />
          </v-window-item>
        </v-window>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="navigateToEditFamily(family.id)">
          {{ t('common.edit') }}
        </v-btn>
        <v-btn color="blue-darken-1" variant="text" @click="closeView">
          {{ t('common.close') }}
        </v-btn>
      </v-card-actions>
    </v-card>
    <v-alert v-else-if="!loading" type="info" class="mt-4" variant="tonal">
      {{ t('common.noData') }}
    </v-alert>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import FamilyForm from '@/components/family/FamilyForm.vue';
import EventTimeline from '@/components/events/EventTimeline.vue';
import type { Family } from '@/types/family';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const familyStore = useFamilyStore();

const family = ref<Family | undefined>(undefined);
const loading = ref(false);
const selectedTab = ref('general');
const readOnly = ref(true); // FamilyDetailView is primarily for viewing

const loadFamily = async () => {
  loading.value = true;
  const familyId = route.params.id as string;
  if (familyId) {
    family.value = await familyStore.fetchItemById(familyId);
  }
  loading.value = false;
};

const navigateToEditFamily = (id: string) => {
  router.push(`/family/edit/${id}`);
};

const closeView = () => {
  router.push('/family');
};

onMounted(() => {
  loadFamily();
});

watch(
  () => route.params.id,
  (newId) => {
    if (newId) {
      loadFamily();
    }
  },
);</script>
