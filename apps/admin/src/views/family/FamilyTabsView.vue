<template>
  <v-card :elevation="0" v-if="familyId" class="mb-4" data-testid="family-detail-view">
    <v-card-text>
      <v-tabs v-model="selectedTab" class="mb-4" fixed-tabs>
        <v-tab fixed-tabs v-for="tab in visibleTabs" :key="tab.value" :value="tab.value"
          :data-testid="`tab-${tab.value}`">{{ tab.text }}</v-tab>
        <v-menu v-if="moreTabs.length">
          <template v-slot:activator="{ props }">
            <v-btn class="align-self-center me-4" height="100%" rounded="0" variant="plain" v-bind="props">
              {{ t('common.more') }}
              <v-icon icon="mdi-menu-down" end></v-icon>
            </v-btn>
          </template>
          <v-list class="bg-grey-lighten-3">
            <v-list-item v-for="tab in moreTabs" :key="tab.value" :title="tab.text"
              @click="selectMoreTab(tab)"></v-list-item>
          </v-list>
        </v-menu>
      </v-tabs>
      <v-window v-model="selectedTab">
        <v-window-item value="general">
          <FamilyDetailView :family-id="familyId" :read-only="true" @open-edit-drawer="handleOpenEditDrawer" />
        </v-window-item>
        <v-window-item value="timeline">
          <EventTimeline :family-id="familyId" />
        </v-window-item>
        <v-window-item value="calendar">
          <EventCalendar :family-id="familyId" />
        </v-window-item>
        <v-window-item value="events">
          <EventListView :family-id="familyId" />
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
        <!-- NEW: Face Search Tab -->
        <v-window-item v-if="canViewFaceDataTab" value="face-search">
          <FaceSearchView :family-id="familyId" />
        </v-window-item>
        <!-- NEW: Family Media Tab -->
        <v-window-item value="family-media">
          <FamilyMediaListView :family-id="familyId" />
        </v-window-item>
        <!-- NEW: Memory Item Tab -->
        <v-window-item value="memory-items">
          <MemoryItemListView :family-id="familyId" />
        </v-window-item>
        <v-window-item value="ai-assistant">
          <AIAssistantView :family-id="familyId" />
        </v-window-item>
        <v-window-item v-if="canManageFamily" value="family-settings">
          <FamilySettingsView :family-id="familyId" />
        </v-window-item>
        <!-- NEW: AI Chat Tab -->

        <!-- NEW: Family Location Tab -->
        <v-window-item value="locations">
          <FamilyLocationListView :family-id="familyId" :allow-add="allowAdd" :allow-edit="allowEdit"
            :allow-delete="allowDelete" />
        </v-window-item>
        <v-window-item value="map">
          <FamilyMapView :family-id="familyId" />
        </v-window-item>
      </v-window>
    </v-card-text>
  </v-card>

  <!-- Edit Family Drawer -->
  <BaseCrudDrawer :model-value="showEditDrawer" @update:model-value="showEditDrawer = $event"
    @close="handleCloseEditDrawer">
    <FamilyEditView v-if="familyId" :family-id="familyId" @close="handleCloseEditDrawer" @saved="handleFamilySaved" />
  </BaseCrudDrawer>


</template>
<script setup lang="ts">
import { ref, onMounted, watch, computed, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import FamilyDetailView from './FamilyDetailView.vue';
import FamilyEditView from './FamilyEditView.vue';
import FamilySettingsView from './FamilySettingsView.vue';
import { TreeChart, } from '@/components/family';
import { EventTimeline, EventCalendar } from '@/components/event';
import MemberListView from '@/views/member/MemberListView.vue';
import MemberFaceListView from '@/views/member-face/MemberFaceListView.vue';
import EventListView from '@/views/event/EventListView.vue';
import FaceSearchView from '@/views/member-face/FaceSearchView.vue'; // NEW

import { useAuth } from '@/composables';
import FamilyMediaListView from '@/views/family-media/FamilyMediaListView.vue';
import FamilyLocationListView from '@/views/family-location/FamilyLocationListView.vue'; // NEW
import FamilyMapView from '@/views/family-location/FamilyMapView.vue';
import MemoryItemListView from '@/views/memory-item/MemoryItemListView.vue'; // NEW: MemoryItemListView
import AIAssistantView from '@/views/ai-assistant/AIAssistantView.vue'; // NEW: AI Assistant View
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useQueryClient } from '@tanstack/vue-query'; // NEW


const { t } = useI18n();
const route = useRoute();
const { state } = useAuth();
const queryClient = useQueryClient(); // NEW

const familyId = computed(() => route.params.id as string);

const allowAdd = computed(() => state.isAdmin.value || state.isFamilyManager.value(familyId.value));
const allowEdit = computed(() => state.isAdmin.value || state.isFamilyManager.value(familyId.value));
const allowDelete = computed(() => state.isAdmin.value || state.isFamilyManager.value(familyId.value));

const showEditDrawer = ref(false);

const handleOpenEditDrawer = (_id: string) => {
  showEditDrawer.value = true;
};

const handleCloseEditDrawer = () => {
  showEditDrawer.value = false;
};

const handleFamilySaved = () => {
  handleCloseEditDrawer();
  queryClient.invalidateQueries({ queryKey: ['families', 'detail', familyId.value] });
};

const canViewFaceDataTab = computed(() => {
  return state.isAdmin.value || state.isFamilyManager.value(familyId.value);
});
const canManageFamily = computed(() => {
  return state.isAdmin.value || state.isFamilyManager.value(familyId.value);
});
interface TabItem {
  value: string;
  text: string;
  condition: boolean;
}
const allTabDefinitions = computed(() => [
  { value: 'general', text: t('member.form.tab.general'), condition: true as boolean },
  { value: 'members', text: t('family.members.title'), condition: true as boolean },
  { value: 'family-tree', text: t('family.tree.title'), condition: true as boolean },
  { value: 'face-recognition', text: t('face.face_data'), condition: canViewFaceDataTab.value as boolean },
  { value: 'face-search', text: t('face.search.title'), condition: canViewFaceDataTab.value as boolean }, // NEW Face Search Tab

  { value: 'events', text: t('event.list.title'), condition: true as boolean },
  { value: 'calendar', text: t('event.view.calendar'), condition: true as boolean },
  { value: 'timeline', text: t('member.form.tab.timeline'), condition: true as boolean },
  { value: 'family-media', text: t('familyMedia.list.pageTitle'), condition: true as boolean },
  { value: 'memory-items', text: t('memoryItem.title'), condition: true as boolean }, // NEW Memory Item Tab
  { value: 'ai-assistant', text: t('aiChat.title'), condition: true as boolean },
  { value: 'locations', text: t('familyLocation.list.title'), condition: true as boolean },
  { value: 'map', text: t('map.viewTitle'), condition: true as boolean }, // NEW Map Tab

  { value: 'family-settings', text: t('family.settings.title'), condition: canManageFamily.value as boolean },
]);
const availableTabs = computed(() => allTabDefinitions.value.filter(tab => tab.condition));
const visibleTabs = ref<TabItem[]>([]);
const moreTabs = ref<TabItem[]>([]);
const selectedTab = ref('general');
const MAX_VISIBLE_TABS = 4;
const MAX_FIXED_TABS = 3;
const initializeTabs = (currentSelectedTabValue: string | null) => {
  const activeTabs = availableTabs.value;
  let actualSelectedTabValue = currentSelectedTabValue;
  if (!actualSelectedTabValue || !activeTabs.some(tab => tab.value === actualSelectedTabValue)) {
    actualSelectedTabValue = activeTabs.length > 0 ? activeTabs[0].value : 'general';
  }
  selectedTab.value = actualSelectedTabValue;
  const newVisible: TabItem[] = [];
  const newMore: TabItem[] = [];
  const fixedTabs = activeTabs.slice(0, MAX_FIXED_TABS);
  newVisible.push(...fixedTabs);
  let remainingTabs = activeTabs.slice(MAX_FIXED_TABS);
  if (newVisible.length < MAX_VISIBLE_TABS && remainingTabs.length > 0) {
    const selectedTabInRemaining = remainingTabs.find(tab => tab.value === actualSelectedTabValue);
    const selectedTabIsFixed = fixedTabs.some(tab => tab.value === actualSelectedTabValue);
    if (selectedTabInRemaining && !selectedTabIsFixed) {
      newVisible.push(selectedTabInRemaining);
      remainingTabs = remainingTabs.filter(tab => tab.value !== actualSelectedTabValue);
    } else {
      newVisible.push(remainingTabs.shift()!);
    }
  }
  newMore.push(...remainingTabs);
  visibleTabs.value = newVisible;
  moreTabs.value = newMore;
};
const selectMoreTab = (tabToSelect: TabItem) => {
  if (visibleTabs.value.some(tab => tab.value === tabToSelect.value) && visibleTabs.value.indexOf(tabToSelect) < MAX_FIXED_TABS) {
    selectedTab.value = tabToSelect.value;
    return;
  }
  const newVisibleTabs = [...visibleTabs.value];
  const newMoreTabs = [...moreTabs.value];
  const swappableTabIndex = MAX_FIXED_TABS;
  let swappedOutTab: TabItem | undefined;
  if (newVisibleTabs.length > swappableTabIndex) {
    swappedOutTab = newVisibleTabs.splice(swappableTabIndex, 1)[0];
    if (swappedOutTab) {
      newMoreTabs.push(swappedOutTab);
    }
  }
  const indexInMore = newMoreTabs.findIndex(tab => tab.value === tabToSelect.value);
  if (indexInMore !== -1) {
    newMoreTabs.splice(indexInMore, 1);
  }
  newVisibleTabs.splice(swappableTabIndex, 0, tabToSelect);
  visibleTabs.value = newVisibleTabs;
  moreTabs.value = newMoreTabs;
  nextTick(() => {
    selectedTab.value = tabToSelect.value;
  });
};
onMounted(() => {
  const savedTab = localStorage.getItem('familyDetailSelectedTab');
  initializeTabs(savedTab);
});
watch(selectedTab, (newTab) => {
  localStorage.setItem('familyDetailSelectedTab', newTab);
  if (!visibleTabs.value.some(tab => tab.value === newTab)) {
    initializeTabs(newTab);
  }
});
watch(availableTabs, () => {
  initializeTabs(selectedTab.value);
}, { deep: true });
</script>
