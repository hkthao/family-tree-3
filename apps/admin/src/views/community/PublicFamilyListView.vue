<template>
  <div data-testid="public-family-list-view">
    <FamilySearch @update:filters="actions.handleFilterUpdate" />
    <FamilyList
      :items="state.families.value"
      :total-items="state.totalItems.value"
      :loading="state.loading.value"
      :items-per-page="state.itemsPerPage.value!"
      :search="state.familyListSearchQuery.value!"
      :sortBy="state.sortBy.value!"
      @update:options="actions.handleListOptionsUpdate"
      @update:itemsPerPage="actions.setItemsPerPage"
      @update:search="actions.handleSearchUpdate"
      @view="actions.navigateToFamilyDetail"
      :hide-actions="true"
    />
  </div>
</template>

<script setup lang="ts">
import { usePublicFamilyList } from '@/composables/family/logic/usePublicFamilyList'; // NEW composable
import { FamilySearch, FamilyList } from '@/components/family';

const { state, actions } = usePublicFamilyList(); // Use the new composable
</script>
