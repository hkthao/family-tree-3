<template>
  <v-card :elevation="0" v-if="family" class="mb-4" data-testid="family-detail-view">
    <v-card-title class="text-h6 d-flex text-center justify-center">
      {{ family.name }}
    </v-card-title>
    <v-card-text>
      <v-tabs v-model="selectedTab" class="mb-4">
        <v-tab value="general" data-testid="tab-general">{{ t('member.form.tab.general') }}</v-tab>
        <v-tab value="timeline" data-testid="tab-timeline">{{ t('member.form.tab.timeline') }}</v-tab>
        <v-tab value="calendar" data-testid="tab-calendar">{{ t('event.view.calendar') }}</v-tab>
        <v-tab value="family-tree" data-testid="tab-family-tree">{{ t('family.tree.title') }}</v-tab>
        <v-tab value="members" data-testid="tab-members">{{ t('family.members.title') }}</v-tab>
        <v-tab value="face-recognition" data-testid="tab-face-recognition">{{ t('face.recognition') }}</v-tab>
      </v-tabs>

      <v-window v-model="selectedTab">
        <v-window-item value="general">
          <FamilyForm :initial-family-data="family" :read-only="true" :title="t('family.detail.title')" />
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="gray" @click="closeView" data-testid="button-close">
              {{ t('common.close') }}
            </v-btn>
            <v-btn color="primary" @click="editDrawer = true" data-testid="button-edit">
              {{ t('common.edit') }}
            </v-btn>
          </v-card-actions>
        </v-window-item>

        <v-window-item value="timeline">
          <EventTimeline :family-id="family.id" :read-only="readOnly" />
        </v-window-item>

        <v-window-item value="calendar">
          <EventCalendar :family-id="family.id" />
        </v-window-item>

        <v-window-item value="family-tree">
          <TreeChart :family-id="family.id" />
        </v-window-item>

        <v-window-item value="members">
          <MemberListView :family-id="family.id" :hide-search="true" />
        </v-window-item>

        <v-window-item value="face-recognition">
          <FaceRecognitionView :family-id="family.id" />
        </v-window-item>
      </v-window>
    </v-card-text>

  </v-card>
  <v-alert v-else-if="!detail.loading" type="info" class="mt-4" variant="tonal">
    {{ t('common.noData') }}
  </v-alert>

  <v-navigation-drawer v-model="editDrawer" location="right" temporary width="650">
    <FamilyEditView v-if="editableFamily && editDrawer" :initial-family="editableFamily" @close="editDrawer = false"
      @saved="handleFamilySaved" />
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { FamilyForm, TreeChart } from '@/components/family';
import { EventTimeline, EventCalendar } from '@/components/event';
import MemberListView from '@/views/member/MemberListView.vue';
import FamilyEditView from '@/views/family/FamilyEditView.vue';
import FaceRecognitionView from '@/views/face/FaceRecognitionView.vue';
import type { Family } from '@/types';
import { storeToRefs } from 'pinia';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const familyStore = useFamilyStore();

const { detail } = storeToRefs(familyStore);

const family = ref<Family | undefined>(undefined);
const editableFamily = ref<Family | undefined>(undefined); // Copy of family for editing
const selectedTab = ref('general');
const readOnly = ref(true); // FamilyDetailView is primarily for viewing
const editDrawer = ref(false); // Control visibility of the edit drawer

const loadFamily = async () => {
  const familyId = route.params.id as string;
  if (familyId) {
    await familyStore.getById(familyId);
    if (!familyStore.error) {
      family.value = familyStore.detail.item as Family;
    } else {
      family.value = undefined; // Clear family on error
    }
  }
};

const handleFamilySaved = async () => {
  editDrawer.value = false;
  await loadFamily(); // Reload family data after saving
  selectedTab.value = 'general'; // Ensure 'general' tab is active
};

const closeView = () => {
  router.push('/family');
};

onMounted(() => {
  loadFamily();
  const savedTab = localStorage.getItem('familyDetailSelectedTab');
  if (savedTab) {
    selectedTab.value = savedTab;
  }
});

watch(selectedTab, (newTab) => {
  localStorage.setItem('familyDetailSelectedTab', newTab);
});

watch(
  () => route.params.id,
  (newId) => {
    if (newId) {
      loadFamily();
    }
  },
);

watch(editDrawer, (newVal) => {
  if (newVal && family.value) {
    editableFamily.value = JSON.parse(JSON.stringify(family.value));
  }
});
</script>