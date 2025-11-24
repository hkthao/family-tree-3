<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 mb-4">{{ t('memory.studio.title', { memberName: member?.fullName }) }}</h1>
        <v-alert v-if="!member" type="info" class="mb-4">{{ t('memory.studio.loadingMember') }}</v-alert>

        <v-tabs
          v-if="member"
          v-model="activeTab"
          align-tabs="center"
          class="mb-4"
        >
          <v-tab value="story">{{ t('aiMemorialStudio.selection.storyMemory') }}</v-tab>
          <v-tab value="photo">{{ t('aiMemorialStudio.selection.photoRevival') }}</v-tab>
          <v-tab value="voice">{{ t('aiMemorialStudio.selection.voiceRevival') }}</v-tab>
        </v-tabs>

        <v-window v-model="activeTab">
          <v-window-item value="story">
            <MemoryDetailPage v-if="memberId" :member-id="memberId" />
            <v-alert v-else type="error">{{ t('common.errors.memberIdRequired') }}</v-alert>
          </v-window-item>
          <v-window-item value="photo">
            <!-- Placeholder for Photo Revival component -->
            <v-card v-if="member">
              <v-card-text>
                <p>Photo Revival Studio for {{ member.fullName }}</p>
                <!-- Add Photo Revival specific components here -->
              </v-card-text>
            </v-card>
            <v-alert v-else type="error">{{ t('common.errors.memberIdRequired') }}</v-alert>
          </v-window-item>
          <v-window-item value="voice">
            <!-- Placeholder for Voice Revival component -->
            <v-card v-if="member">
              <v-card-text>
                <p>Voice Revival Studio for {{ member.fullName }}</p>
                <!-- Add Voice Revival specific components here -->
              </v-card-text>
            </v-card>
            <v-alert v-else type="error">{{ t('common.errors.memberIdRequired') }}</v-alert>
          </v-window-item>
        </v-window>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store'; // To fetch member details
import type { Member } from '@/types';
import MemoryDetailPage from './MemoryDetailPage.vue';

const route = useRoute();
const { t } = useI18n();
const memberStore = useMemberStore();

const memberId = ref<string | null>(null);
const member = ref<Member | null>(null);
const activeTab = ref('story'); // Default tab

const fetchMemberDetails = async (id: string) => {
  member.value = await memberStore.getById(id) || null;
};

watch(() => route.params.memberId, async (newMemberId) => {
  if (typeof newMemberId === 'string') {
    memberId.value = newMemberId;
    await fetchMemberDetails(newMemberId);
  } else {
    memberId.value = null;
    member.value = null;
  }
}, { immediate: true });

watch(() => route.params.aiMemorialStudioType, (newType) => {
  if (typeof newType === 'string') {
    activeTab.value = newType;
  }
}, { immediate: true });

onMounted(() => {
  if (typeof route.params.memberId === 'string') {
    memberId.value = route.params.memberId;
    fetchMemberDetails(route.params.memberId);
  }
  if (typeof route.params.aiMemorialStudioType === 'string') {
    activeTab.value = route.params.aiMemorialStudioType;
  }
});
</script>