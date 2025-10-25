<template>
  <v-card v-if="family" class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ family.name }}
      <v-spacer></v-spacer>
    </v-card-title>
    <v-card-text>
      <v-tabs v-model="selectedTab" class="mb-4">
        <v-tab value="general" data-testid="tab-general">{{ t('member.form.tab.general') }}</v-tab>
        <v-tab value="timeline" data-testid="tab-timeline">{{ t('member.form.tab.timeline') }}</v-tab>
        <v-tab value="calendar" data-testid="tab-calendar">{{ t('event.view.calendar') }}</v-tab>
        <v-tab value="family-tree" data-testid="tab-family-tree">{{ t('family.tree.title') }}</v-tab>
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
          <EventTimeline :family-id="family.id" :read-only="readOnly" />
        </v-window-item>

        <v-window-item value="calendar">
          <EventCalendar :family-id="family.id" :read-only="readOnly" />
        </v-window-item>

        <v-window-item value="family-tree">
          <TreeChart :family-id="family.id" />
        </v-window-item>
      </v-window>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <v-btn color="primary" @click="navigateToEditFamily(family.id)" data-testid="button-edit">
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
import { useFamilyStore } from '@/stores/family.store';
import { FamilyForm, TreeChart } from '@/components/family';
import { EventTimeline, EventCalendar } from '@/components/event';
import type { Family } from '@/types';

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
    await familyStore.getById(familyId);
    if (!familyStore.error) {
      family.value = familyStore.currentItem as Family;
    } else {
      family.value = undefined; // Clear family on error
    }
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
);
</script>