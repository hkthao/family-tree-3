import { ref, computed, watch, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useAuth } from '@/composables';
import { useQueryClient } from '@tanstack/vue-query';
import type { FamilyDto } from '@/types';
import type { TabItem } from './familyTabs.logic'; // Import TabItem
import { calculateTabVisibility, calculateSwappedTabs, MAX_VISIBLE_TABS, MAX_FIXED_TABS } from './familyTabs.logic';
import type { ILocalStorageAdapter } from './localStorage.adapter'; // Import adapter
import { defaultLocalStorageAdapter } from './localStorage.adapter'; // Import adapter

interface UseFamilyTabsProps {
  familyId: Ref<string>;
  familyData: Ref<FamilyDto | undefined>; // Pass familyData as a prop
}

interface UseFamilyTabsDeps {
  useI18n: typeof useI18n;
  useRoute: typeof useRoute;
  useRouter: typeof useRouter;
  useAuth: typeof useAuth;
  useQueryClient: typeof useQueryClient;
  localStorageAdapter: ILocalStorageAdapter; // Add to deps
}

const defaultDeps: UseFamilyTabsDeps = {
  useI18n,
  useRoute,
  useRouter,
  useAuth,
  useQueryClient,
  localStorageAdapter: defaultLocalStorageAdapter, // Add to defaultDeps
};

export function useFamilyTabs(
  props: UseFamilyTabsProps,
  emit: (event: 'open-edit-drawer', id: string) => void,
  deps: UseFamilyTabsDeps = defaultDeps,
) {
  const { useI18n, useAuth, useQueryClient, localStorageAdapter } = deps;
  const { t } = useI18n();
  const { state: authState } = useAuth();
  const queryClient = useQueryClient();

  // State for the edit drawer
  const showEditDrawer = ref(false);

  // --- Permissions/Access Computations ---
  const allowAdd = computed(() => authState.isAdmin.value || authState.isFamilyManager.value(props.familyId.value));
  const allowEdit = computed(() => authState.isAdmin.value || authState.isFamilyManager.value(props.familyId.value));
  const allowDelete = computed(() => authState.isAdmin.value || authState.isFamilyManager.value(props.familyId.value));
  const canViewFaceDataTab = computed(() => authState.isAdmin.value || authState.isFamilyManager.value(props.familyId.value));
  const canManageFamily = computed(() => authState.isAdmin.value || authState.isFamilyManager.value(props.familyId.value));

  // --- Tab Management Logic ---
  const allTabDefinitions = computed<TabItem[]>(() => [
    { value: 'general', text: t('member.form.tab.general'), condition: true },
    { value: 'members', text: t('family.members.title'), condition: true },
    { value: 'family-tree', text: t('family.tree.title'), condition: true },
    { value: 'face-recognition', text: t('face.face_data'), condition: canViewFaceDataTab.value },
    { value: 'face-search', text: t('face.search.title'), condition: canViewFaceDataTab.value },
    { value: 'events', text: t('event.list.title'), condition: true },
    { value: 'calendar', text: t('event.view.calendar'), condition: true },
    { value: 'timeline', text: t('member.form.tab.timeline'), condition: true },
    { value: 'family-media', text: t('familyMedia.list.pageTitle'), condition: true },
    { value: 'memory-items', text: t('memoryItem.title'), condition: true },
    { value: 'locations', text: t('familyLocation.list.title'), condition: true },
    { value: 'map', text: t('map.viewTitle'), condition: true },
    { value: 'family-settings', text: t('family.settings.title'), condition: canManageFamily.value },
  ]);

  const availableTabs = computed(() => allTabDefinitions.value.filter(tab => tab.condition));
  const visibleTabs = ref<TabItem[]>([]);
  const moreTabs = ref<TabItem[]>([]);
  const selectedTab = ref<string | null>('general');

  // --- Functions ---
  const initializeTabs = (currentSelectedTabValue: string | null) => {
    const activeTabs = availableTabs.value;
    const { visibleTabs: newVisible, moreTabs: newMore, actualSelectedTabValue } = calculateTabVisibility(
      activeTabs,
      currentSelectedTabValue,
      MAX_VISIBLE_TABS,
      MAX_FIXED_TABS
    );

    selectedTab.value = actualSelectedTabValue;
    visibleTabs.value = newVisible;
    moreTabs.value = newMore;
  };

  const selectMoreTab = (tabToSelect: TabItem) => {
    if (visibleTabs.value.some((tab: TabItem) => tab.value === tabToSelect.value) && visibleTabs.value.indexOf(tabToSelect) < MAX_FIXED_TABS) {
      selectedTab.value = tabToSelect.value;
      return;
    }

    const { newVisibleTabs, newMoreTabs } = calculateSwappedTabs(
      visibleTabs.value,
      moreTabs.value,
      tabToSelect,
      MAX_FIXED_TABS
    );

    visibleTabs.value = newVisibleTabs;
    moreTabs.value = newMoreTabs;
    selectedTab.value = tabToSelect.value;
  };

  const handleOpenEditDrawer = (_id: string) => {
    showEditDrawer.value = true;
    emit('open-edit-drawer', _id);
  };

  const handleCloseEditDrawer = () => {
    showEditDrawer.value = false;
  };

  const handleFamilySaved = () => {
    handleCloseEditDrawer();
    queryClient.invalidateQueries({ queryKey: ['families', 'detail', props.familyId.value] });
  };


  // --- Watchers ---
  const handleSelectedTabChange = (newTab: string | null) => {
    if (newTab !== null) {
      localStorageAdapter.setItem('familyDetailSelectedTab', newTab);
    } else {
        localStorageAdapter.removeItem('familyDetailSelectedTab');
    }
    // Ensure that if the newTab is not currently visible, we re-initialize to make it visible.
    // This handles cases where a tab from 'moreTabs' is selected.
    if (!visibleTabs.value.some((tab: TabItem) => tab.value === newTab)) {
      initializeTabs(newTab);
    }
  };

  const handleAvailableTabsChange = () => {
    initializeTabs(selectedTab.value);
  };

  watch(selectedTab, handleSelectedTabChange);

  watch(availableTabs, handleAvailableTabsChange, { deep: true });


  // --- Lifecycle Hook - onMounted equivalent ---
  // The consuming component will call initialize onMounted.
  const initialize = () => {
    const savedTab = localStorageAdapter.getItem('familyDetailSelectedTab');
    initializeTabs(savedTab);
  };

  return {
    state: {
      selectedTab,
      visibleTabs,
      moreTabs,
      showEditDrawer,
      familyId: props.familyId,
      allowAdd,
      allowEdit,
      allowDelete,
      canViewFaceDataTab,
      canManageFamily,
    },
    actions: {
      selectMoreTab,
      handleOpenEditDrawer,
      handleCloseEditDrawer,
      handleFamilySaved,
      initialize, // Expose initialize for onMounted in consuming component
    },
    t, // Expose t for i18n
  };
}
