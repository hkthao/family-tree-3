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
          <FamilyDetail :family-id="familyId" :read-only="readOnlyValue" />
        </v-window-item>

        <v-window-item value="timeline">
          <EventTimeline :family-id="familyId" :read-only="readOnlyValue" />
        </v-window-item>

        <v-window-item value="calendar">
          <EventCalendar :family-id="familyId" :events="familyEvents || []" :loading="isLoadingFamilyEvents" />
        </v-window-item>

        <v-window-item value="events">
          <EventListView :family-id="familyId" :read-only="readOnlyValue" />
        </v-window-item>

        <v-window-item value="member-stories">
          <MemberStoryListView :family-id="familyId" :read-only="readOnlyValue" />
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

        <!-- NEW: Family Media Tab -->
        <v-window-item value="family-media">
          <FamilyMediaListView :family-id="familyId" />
        </v-window-item>

        <!-- Family Link Requests and Linking are commented out in original, keeping them as-is -->
        <!-- <v-window-item v-if="canManageFamily" value="family-link-requests">
          <FamilyLinkRequestsListView :family-id="familyId" />
        </v-window-item>

        <v-window-item v-if="canManageFamily" value="family-linking">
          <FamilyLinkListView :family-id="familyId" :read-only="readOnly" />
        </v-window-item> -->

        <v-window-item v-if="canManageFamily" value="family-settings">
          <FamilySettingsTab :family-id="familyId" />
        </v-window-item>
      </v-window>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import FamilyDetail from '@/components/family/FamilyDetail.vue';
import { TreeChart, FamilySettingsTab } from '@/components/family';
import { EventTimeline, EventCalendar } from '@/components/event';
import MemberListView from '@/views/member/MemberListView.vue';
import MemberFaceListView from '@/views/member-face/MemberFaceListView.vue';
import EventListView from '@/views/event/EventListView.vue';
import MemberStoryListView from '@/views/member-story/MemberStoryListView.vue';
import { useAuth } from '@/composables';
import FamilyMediaListView from '@/views/family-media/FamilyMediaListView.vue';
import { useUpcomingEvents } from '@/composables/data/useUpcomingEvents'; // Import useUpcomingEvents

const { t } = useI18n();
const route = useRoute();
const { isAdmin, isFamilyManager } = useAuth();

const familyId = computed(() => route.params.id as string);
const readOnlyValue = true; // Use a plain boolean const

const { upcomingEvents: familyEvents, isLoading: isLoadingFamilyEvents } = useUpcomingEvents(familyId);

const canViewFaceDataTab = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

const canManageFamily = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

interface TabItem {
  value: string;
  text: string;
  condition: boolean;
}

// Define all possible tabs
const allTabDefinitions = computed(() => [
  { value: 'general', text: t('member.form.tab.general'), condition: true as boolean },
  { value: 'members', text: t('family.members.title'), condition: true as boolean },
  { value: 'family-tree', text: t('family.tree.title'), condition: true as boolean },
  { value: 'face-recognition', text: t('face.face_data'), condition: canViewFaceDataTab.value as boolean },
  { value: 'member-stories', text: t('memberStory.list.title'), condition: true as boolean },
  { value: 'events', text: t('event.list.title'), condition: true as boolean },
  { value: 'calendar', text: t('event.view.calendar'), condition: true as boolean },
  { value: 'timeline', text: t('member.form.tab.timeline'), condition: true as boolean },
  { value: 'family-media', text: t('familyMedia.list.pageTitle'), condition: true as boolean }, // NEW: Family Media Tab
  // Keeping commented out tabs as they were in the original file
  // { value: 'family-link-requests', text: t('familyLinkRequest.list.title'), condition: canManageFamily.value as boolean },
  // { value: 'family-linking', text: t('familyLink.list.title'), condition: canManageFamily.value as boolean },
  { value: 'family-settings', text: t('family.settings.title'), condition: canManageFamily.value as boolean },
]);

// Filter tabs based on their conditions
const availableTabs = computed(() => allTabDefinitions.value.filter(tab => tab.condition));

const visibleTabs = ref<TabItem[]>([]);
const moreTabs = ref<TabItem[]>([]);
const selectedTab = ref('general'); // Initialize with a default value

const MAX_VISIBLE_TABS = 4; // Number of tabs to show initially
const MAX_FIXED_TABS = 3; // Number of initial tabs that are fixed and cannot be swapped

// Function to initialize visible and more tabs
const initializeTabs = (currentSelectedTabValue: string | null) => {
  const activeTabs = availableTabs.value;
  let actualSelectedTabValue = currentSelectedTabValue;

  // Ensure actualSelectedTabValue is valid and among activeTabs, default if not
  if (!actualSelectedTabValue || !activeTabs.some(tab => tab.value === actualSelectedTabValue)) {
    actualSelectedTabValue = activeTabs.length > 0 ? activeTabs[0].value : 'general';
  }
  selectedTab.value = actualSelectedTabValue;

  const newVisible: TabItem[] = [];
  const newMore: TabItem[] = [];

  // 1. Add fixed tabs to newVisible
  // Take a slice to avoid modifying activeTabs directly for the loop conditions
  const fixedTabs = activeTabs.slice(0, MAX_FIXED_TABS);
  newVisible.push(...fixedTabs);

  // 2. Determine remaining tabs (candidates for the 4th swappable tab and more)
  let remainingTabs = activeTabs.slice(MAX_FIXED_TABS);

  // 3. Handle the 4th visible tab (the swappable one)
  // Ensure we only try to fill the 4th slot if it's available and there are tabs to fill it with.
  if (newVisible.length < MAX_VISIBLE_TABS && remainingTabs.length > 0) {
    const selectedTabInRemaining = remainingTabs.find(tab => tab.value === actualSelectedTabValue);
    const selectedTabIsFixed = fixedTabs.some(tab => tab.value === actualSelectedTabValue);

    if (selectedTabInRemaining && !selectedTabIsFixed) {
      // If the currently selected tab is among the remaining and not a fixed tab,
      // bring it to the 4th visible position.
      newVisible.push(selectedTabInRemaining);
      remainingTabs = remainingTabs.filter(tab => tab.value !== actualSelectedTabValue);
    } else {
      // Otherwise, just take the next available tab from remaining as the 4th tab.
      newVisible.push(remainingTabs.shift()!);
    }
  }

  // 4. Add any leftover tabs (including the one swapped out if selectedTab was fixed) to newMore
  newMore.push(...remainingTabs);

  visibleTabs.value = newVisible;
  moreTabs.value = newMore;
};

// Function to handle selecting a tab from the "more" menu
const selectMoreTab = (tabToSelect: TabItem) => {
  // If the tab is already visible and it's a fixed tab, simply select it.
  // If it's the 4th tab (swappable), it will be handled by the swap logic below.
  if (visibleTabs.value.some(tab => tab.value === tabToSelect.value) && visibleTabs.value.indexOf(tabToSelect) < MAX_FIXED_TABS) {
    selectedTab.value = tabToSelect.value;
    return;
  }

  // Create new arrays to ensure reactivity updates correctly
  const newVisibleTabs = [...visibleTabs.value];
  const newMoreTabs = [...moreTabs.value];

  // The swappable tab is at index MAX_FIXED_TABS (which is 3 for the 4th tab)
  const swappableTabIndex = MAX_FIXED_TABS;
  let swappedOutTab: TabItem | undefined;

  // 1. Remove the current swappable tab from visibleTabs and add to moreTabs
  if (newVisibleTabs.length > swappableTabIndex) {
    swappedOutTab = newVisibleTabs.splice(swappableTabIndex, 1)[0]; // Remove the 4th tab
    if (swappedOutTab) {
      newMoreTabs.push(swappedOutTab); // Add it to newMoreTabs
    }
  }

  // 2. Remove selected tab from newMoreTabs (if it exists there)
  const indexInMore = newMoreTabs.findIndex(tab => tab.value === tabToSelect.value);
  if (indexInMore !== -1) {
    newMoreTabs.splice(indexInMore, 1);
  }

  // 3. Insert the newly selected tab into the 4th position of visibleTabs
  newVisibleTabs.splice(swappableTabIndex, 0, tabToSelect);
  
  // Reassign the refs with the new arrays
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

// Watch selectedTab to save to local storage and re-arrange tabs if necessary
watch(selectedTab, (newTab) => {
  localStorage.setItem('familyDetailSelectedTab', newTab);
  // If the new tab is not in the currently visible tabs, re-initialize to make it visible
  if (!visibleTabs.value.some(tab => tab.value === newTab)) {
    initializeTabs(newTab);
  }
});

// Watch availableTabs to react to changes in conditions (e.g., user roles change)
watch(availableTabs, () => {
  initializeTabs(selectedTab.value); // Re-initialize if available tabs change
}, { deep: true }); // Deep watch is important for array changes
</script>
