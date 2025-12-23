export interface TabItem {
  value: string;
  text: string;
  condition: boolean;
}

export const MAX_VISIBLE_TABS = 4;
export const MAX_FIXED_TABS = 3;

/**
 * Calculates the visible and "more" tabs based on the available tabs and the currently selected tab.
 * This is a pure function that does not interact with Vue's reactivity system directly.
 *
 * @param activeTabs An array of all available TabItem objects that meet their conditions.
 * @param currentSelectedTabValue The value of the currently selected tab, or null if none is selected.
 * @param maxVisible The maximum number of tabs that should be visible.
 * @param maxFixed The number of tabs that are always fixed at the beginning.
 * @returns An object containing the calculated visibleTabs, moreTabs, and the actual selectedTabValue.
 */
export function calculateTabVisibility(
  activeTabs: TabItem[],
  currentSelectedTabValue: string | null,
  maxVisible: number = MAX_VISIBLE_TABS,
  maxFixed: number = MAX_FIXED_TABS
): { visibleTabs: TabItem[]; moreTabs: TabItem[]; actualSelectedTabValue: string | null } {

  let actualSelectedTabValue = currentSelectedTabValue;

  // Determine the actual selected tab value, defaulting to the first active tab if current is invalid
  if (!actualSelectedTabValue || !activeTabs.some(tab => tab.value === actualSelectedTabValue)) {
    actualSelectedTabValue = activeTabs.length > 0 ? activeTabs[0].value : null; // Changed to null if no active tabs
  }

  const newVisible: TabItem[] = [];
  let newMore: TabItem[] = [];

  const fixedTabs = activeTabs.slice(0, maxFixed);
  newVisible.push(...fixedTabs);

  let remainingTabs = activeTabs.slice(maxFixed);

  // If there's space and remaining tabs, try to include the selected tab or the next available
  if (newVisible.length < maxVisible && remainingTabs.length > 0) {
    const selectedTabInRemaining = remainingTabs.find(tab => tab.value === actualSelectedTabValue);
    const selectedTabIsFixed = fixedTabs.some(tab => tab.value === actualSelectedTabValue);

    if (selectedTabInRemaining && !selectedTabIsFixed) {
      newVisible.push(selectedTabInRemaining);
      remainingTabs = remainingTabs.filter(tab => tab.value !== actualSelectedTabValue);
    } else if (remainingTabs.length > 0) {
      // If selected tab isn't in remaining or is already fixed, just push the next remaining tab
      newVisible.push(remainingTabs.shift()!);
    }
  }

  newMore = [...remainingTabs];

  return {
    visibleTabs: newVisible,
    moreTabs: newMore,
    actualSelectedTabValue: actualSelectedTabValue,
  };
}

/**
 * Calculates the new state of visible and "more" tabs after selecting a tab from the "more" list.
 *
 * @param currentVisibleTabs The currently visible tabs.
 * @param currentMoreTabs The currently "more" tabs.
 * @param tabToSelect The TabItem that was selected from the "more" list.
 * @param maxFixed The number of tabs that are always fixed at the beginning.
 * @returns An object containing the newVisibleTabs and newMoreTabs.
 */
export function calculateSwappedTabs(
  currentVisibleTabs: TabItem[],
  currentMoreTabs: TabItem[],
  tabToSelect: TabItem,
  maxFixed: number = MAX_FIXED_TABS
): { newVisibleTabs: TabItem[]; newMoreTabs: TabItem[] } {
  const newVisibleTabs = [...currentVisibleTabs];
  const newMoreTabs = [...currentMoreTabs];
  const swappableTabIndex = maxFixed;

  // If the tab to select is already visible within the fixed section, no swap is needed for arrangement
  // This function assumes the calling context will handle updating selectedTab.
  if (newVisibleTabs.some(tab => tab.value === tabToSelect.value) && newVisibleTabs.indexOf(tabToSelect) < maxFixed) {
    return { newVisibleTabs: currentVisibleTabs, newMoreTabs: currentMoreTabs };
  }

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

  return { newVisibleTabs, newMoreTabs };
}
