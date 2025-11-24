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
              :items-length="totalMembers" :loading="loadingMembers" @update:options="loadMembers" class="elevation-0">
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
import { ref, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useMemberStore } from '@/stores/member.store';
import { FamilyAutocomplete } from '@/components/common';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue';
import type { MemberListDto } from '@/types'; // MemberListDto from '@/types'

const { t } = useI18n();
const router = useRouter();
const memberStore = useMemberStore();

const selectedFamilyId = ref<string | null>(null);
const searchMember = ref('');
const members = ref<MemberListDto[]>([]);
const totalMembers = ref(0);
const loadingMembers = ref(false);
const itemsPerPage = ref(10);
const selectingMember = ref<string | null>(null); // To track which member is being selected

const headers = ref([
  { title: t('member.list.headers.avatar'), key: 'avatarUrl', sortable: false },
  { title: t('member.list.headers.fullName'), key: 'fullName' },
  { title: t('member.list.headers.gender'), key: 'gender' },
  { title: t('member.list.headers.birthDeathYears'), key: 'birthDeathYears' },
  { title: t('aiMemorialStudio.selection.actions'), key: 'actions', sortable: false },
]);

interface LoadMembersOptions {
  page: number;
  itemsPerPage: number;
  sortBy?: string | null;
}

const loadMembers = async (options: LoadMembersOptions) => {
  if (!selectedFamilyId.value) {
    members.value = [];
    totalMembers.value = 0;
    return;
  }

  loadingMembers.value = true;
  const result = await memberStore.searchMembers({
    familyId: selectedFamilyId.value,
    searchQuery: searchMember.value,
    page: options.page,
    itemsPerPage: options.itemsPerPage,
    sortBy: options.sortBy,
  });

  if (result.isSuccess) {
    members.value = result.value?.items || [];
    totalMembers.value = result.value?.totalCount || 0;
  } else {
    members.value = [];
    totalMembers.value = 0;
  }
  loadingMembers.value = false;
};

const handleFamilySelection = (familyId: string | null) => {
  selectedFamilyId.value = familyId;
  if (familyId) {
    loadMembers({ page: 1, itemsPerPage: itemsPerPage.value });
  } else {
    members.value = [];
    totalMembers.value = 0;
  }
};

const selectMember = async (member: MemberListDto) => {
  selectingMember.value = member.id;
  // Navigate to the member's memories studio
  await router.push({ name: 'MemberMemories', params: { memberId: member.id } });
  selectingMember.value = null;
};

onMounted(() => {
  // If a family ID is already selected (e.g., from query params or previous state)
  // this would be a good place to load members initially.
  // For now, it will load when selectedFamilyId changes.
});

watch(searchMember, () => {
  loadMembers({ page: 1, itemsPerPage: itemsPerPage.value });
});

watch(selectedFamilyId, () => {
  loadMembers({ page: 1, itemsPerPage: itemsPerPage.value });
});
</script>

<style scoped>
/* Add any specific styles */
</style>
