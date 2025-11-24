<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 mb-4">{{ t('aiMemorialStudio.selection.title') }}</h1>
        <p class="text-subtitle-1 text-grey-darken-1">{{ t('aiMemorialStudio.selection.description') }}</p>
      </v-col>
    </v-row>

    <v-row class="mt-4">
      <v-col cols="12" md="6">
        <FamilyAutocomplete
          v-model="selectedFamilyId"
          :label="t('aiMemorialStudio.selection.selectFamily')"
          clearable
          @update:modelValue="handleFamilySelection"
          :key="'ai-memorial-studio-family-autocomplete'"
        />
      </v-col>
    </v-row>

    <v-row v-if="selectedFamilyId">
      <v-col cols="12">
        <v-card flat>
          <v-card-title class="d-flex align-center">
            <span class="text-h6">{{ t('member.list.title') }}</span>
            <v-spacer></v-spacer>
            <v-text-field v-model="searchMember" append-inner-icon="mdi-magnify" :label="t('common.search')" single-line
              hide-details density="compact" class="flex-grow-0" style="max-width: 200px;"></v-text-field>
          </v-card-title>
          <v-card-text>
            <v-data-table-server v-model:items-per-page="itemsPerPage" :headers="headers" :items="members"
              :items-length="totalMembers" :loading="loadingMembers" @update:options="({ page, itemsPerPage, sortBy }) => loadMembers({ page, itemsPerPage, sortBy })" class="elevation-0">
              <template v-slot:item.avatarUrl="{ item }">
                <AvatarDisplay :src="item.avatarUrl" :gender="item.gender" :size="36" />
              </template>
              <template v-slot:item.fullName="{ item }">
                {{ item.fullName }}
              </template>
              <template v-slot:item.birthDeathYears="{ item }">
                {{ item.birthDeathYears }}
              </template>
              <template v-slot:item.actions="{ item }">
                <v-btn color="primary" @click="selectMember(item)" :loading="selectingMember === item.id">
                  {{ t('aiMemorialStudio.selection.selectMember') }}
                </v-btn>
              </template>
              <template v-slot:no-data>
                <v-alert type="info">{{ t('member.list.noMembers') }}</v-alert>
              </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <v-row v-else>
      <v-col cols="12">
        <v-alert type="info">{{ t('aiMemorialStudio.selection.noFamilySelected') }}</v-alert>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { FamilyAutocomplete } from '@/components/common';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue';
import { useAIMemorialStudioStore } from '@/stores/aiMemorialStudio.store';

const { t } = useI18n();
const aiMemorialStudioStore = useAIMemorialStudioStore();

// Map state and getters from the store
const {
  selectedFamilyId,
  searchMember,
  members,
  totalMembers,
  loadingMembers,
  itemsPerPage,
  selectingMember,
  headers,
} = storeToRefs(aiMemorialStudioStore);

// Map actions from the store
const {
  loadMembers,
  handleFamilySelection,
  selectMember,
} = aiMemorialStudioStore;

// Watch for changes in searchMember and selectedFamilyId
watch(searchMember, () => {
  loadMembers({ page: 1, itemsPerPage: itemsPerPage.value });
});

watch(selectedFamilyId, () => {
  loadMembers({ page: 1, itemsPerPage: itemsPerPage.value });
});

onMounted(() => {
  aiMemorialStudioStore.init(); // Initialize the store
});
</script>

<style scoped>
/* Add any specific styles */
</style>
