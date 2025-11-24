<template>
  <v-container>
    <v-row>
      <v-col cols="12" class="d-flex align-center">
        <v-btn icon @click="$router.back()" variant="text">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
        <AvatarDisplay :src="memberAvatarSrc" :size="48" class="mr-3" />
        <h1 class="text-h5">{{ t('memory.studio.title', { memberName: member?.fullName || '...' }) }}</h1>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="openCreateMemory" data-testid="create-memory-button">
          {{ t('memory.studio.addMemory') }}
        </v-btn>
      </v-col>
    </v-row>

    <v-row v-if="member">
      <v-col cols="12">
        <!-- MemoryList component will go here -->
        <MemoryList :member-id="member.id" />
      </v-col>
    </v-row>
    <v-row v-else>
      <v-col cols="12" class="text-center">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">{{ t('memory.studio.loadingMember') }}</p>
      </v-col>
    </v-row>

    <!-- Create Memory Drawer -->
    <BaseCrudDrawer v-model="createMemoryDrawer" @close="closeCreateMemory">
      <MemoryCreate v-if="createMemoryDrawer" :member-id="memberId" @close="closeCreateMemory" @saved="handleMemorySaved" />
    </BaseCrudDrawer>
  </v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'; // Removed ref
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { useCrudDrawer } from '@/composables/useCrudDrawer';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { Gender } from '@/types';
import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';

// Import new Memory components
import MemoryList from '@/components/memory/MemoryList.vue';
import MemoryCreate from '@/components/memory/MemoryCreate.vue';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const memberStore = useMemberStore();

const memberId = computed(() => route.params.memberId as string);
const member = computed(() => memberStore.detail.item);

const { addDrawer: createMemoryDrawer, openAddDrawer: openCreateMemory, closeAddDrawer: closeCreateMemory } = useCrudDrawer();

const memberAvatarSrc = computed(() => {
  if (member.value?.avatarUrl) {
    return member.value.avatarUrl;
  }
  if (member.value?.gender === Gender.Male) {
    return maleAvatar;
  }
  if (member.value?.gender === Gender.Female) {
    return femaleAvatar;
  }
  return maleAvatar; // Fallback
});

const loadMember = async (id: string) => {
  await memberStore.getById(id);
  if (!member.value) {
    // Redirect to 404 or show error if member not found
    router.push({ name: 'NotFound' });
  }
};

const handleMemorySaved = () => {
  closeCreateMemory();
  // Optionally reload memory list here if needed
  // For now, MemoryList will handle its own loading.
};

onMounted(() => {
  if (memberId.value) {
    loadMember(memberId.value);
  }
});

watch(
  () => route.params.memberId,
  (newMemberId) => {
    if (newMemberId) {
      loadMember(newMemberId as string);
    }
  },
);
</script>

<style scoped>
/* Add any specific styles */
</style>
