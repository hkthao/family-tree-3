<template>
  <v-card :elevation="0" v-if="familyId" class="mb-4" data-testid="family-detail-view">
    <v-card-text>
      <v-tabs v-model="selectedTab" class="mb-4">
        <v-tab value="general" data-testid="tab-general">{{ t('member.form.tab.general') }}</v-tab>
        <v-tab value="members" data-testid="tab-members">{{ t('family.members.title') }}</v-tab>
        <v-tab value="family-tree" data-testid="tab-family-tree">{{ t('family.tree.title') }}</v-tab>
        <v-tab v-if="canViewFaceDataTab" value="face-recognition" data-testid="tab-face-recognition">{{
          t('face.face_data') }}</v-tab>
        <v-tab value="member-stories" data-testid="tab-member-stories">{{ t('memberStory.list.title') }}</v-tab>
        <v-tab value="events" data-testid="tab-events">{{ t('event.list.title') }}</v-tab>
        <v-tab value="calendar" data-testid="tab-calendar">{{ t('event.view.calendar') }}</v-tab>
        <v-tab value="timeline" data-testid="tab-timeline">{{ t('member.form.tab.timeline') }}</v-tab>
        <v-tab v-if="canManageFamily" value="family-settings" data-testid="tab-family-settings">{{
          t('family.settings.title') }}</v-tab>
      </v-tabs>

      <v-window v-model="selectedTab">
        <v-window-item value="general">
          <FamilyDetail :family-id="familyId" :read-only="readOnly" />
        </v-window-item>

        <v-window-item value="timeline">
          <EventTimeline :family-id="familyId" :read-only="readOnly" />
        </v-window-item>

        <v-window-item value="calendar">
          <EventCalendar :family-id="familyId" />
        </v-window-item>

        <v-window-item value="events">
          <EventListView :family-id="familyId" :read-only="readOnly" />
        </v-window-item>

        <v-window-item value="member-stories">
          <MemberStoryListView :family-id="familyId" :read-only="readOnly" />
        </v-window-item>

        <v-window-item value="family-tree">
          <TreeChart :family-id="familyId" />
        </v-window-item>

        <v-window-item value="members">
          <MemberListView :family-id="familyId" :hide-search="true" />
        </v-window-item>

        <v-window-item v-if="canViewFaceDataTab" value="face-recognition">
          <MemberFaceListView :hideSearch="true" :family-id="familyId" />
        </v-window-item>

        <v-window-item v-if="canManageFamily" value="family-settings">
          <FamilySettingsTab :family-id="familyId" />
        </v-window-item>
      </v-window>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import { TreeChart, FamilyDetail, FamilySettingsTab } from '@/components/family';
import { EventTimeline, EventCalendar } from '@/components/event';
import MemberListView from '@/views/member/MemberListView.vue';
import MemberFaceListView from '@/views/member-face/MemberFaceListView.vue';
import EventListView from '@/views/event/EventListView.vue';
import MemberStoryListView from '@/views/member-story/MemberStoryListView.vue';
import { useAuth } from '@/composables/useAuth';

const { t } = useI18n();
const route = useRoute();
const { isAdmin, isFamilyManager } = useAuth();

const selectedTab = ref('general');
const readOnly = ref(true);
const familyId = computed(() => route.params.id as string);

const canViewFaceDataTab = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

const canManageFamily = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

onMounted(() => {
  const savedTab = localStorage.getItem('familyDetailSelectedTab');
  if (savedTab) {
    selectedTab.value = savedTab;
  }
});

watch(selectedTab, (newTab) => {
  localStorage.setItem('familyDetailSelectedTab', newTab);
});

</script>