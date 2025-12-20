<template>
  <div data-testid="family-list-view">
    <FamilySearch id="tour-step-1" @update:filters="actions.handleFilterUpdate" />
    <FamilyList
      id="tour-step-2"
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
      @delete="actions.confirmDelete"
      @create="actions.openAddDrawer"
    />

    <!-- Add Family Drawer -->
    <BaseCrudDrawer v-model="state.addDrawer.value" @close="actions.handleFamilyAddClosed">
      <FamilyAddView v-if="state.addDrawer" @close="actions.handleFamilyAddClosed" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { useFamilyList } from '@/composables/family/logic/useFamilyList';
import { FamilySearch, FamilyList } from '@/components/family';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import FamilyAddView from './FamilyAddView.vue';

const { state, actions } = useFamilyList();
</script>
