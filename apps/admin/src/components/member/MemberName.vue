<template>
  <v-chip v-if="memberData" size="small" class="ma-1">
    <v-avatar start>
      <v-img :src="getMemberAvatar(memberData)" :alt="memberData.fullName"></v-img>
    </v-avatar>
    {{ memberData.fullName }}
  </v-chip>
  <span v-else-if="memberId" class="text-caption text-medium-emphasis">Loading...</span>
  <span v-else></span>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types';
import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';

const props = defineProps<{
  memberId?: string;
}>();

const { t } = useI18n();
const memberStore = useMemberStore();
const memberData = ref<Member | null>(null);

const getMemberAvatar = (member: Member) => {
  if (member.avatarUrl) {
    return member.avatarUrl;
  }
  if (member.gender === 'Male') {
    return maleAvatar;
  }
  if (member.gender === 'Female') {
    return femaleAvatar;
  }
  return maleAvatar; // Fallback for 'Other' or undefined gender
};

const fetchMemberData = async (id?: string) => {
  if (id) {
    const member = await memberStore.getById(id);
    memberData.value = member || null;
  } else {
    memberData.value = null;
  }
};

watch(() => props.memberId, (newId) => {
  fetchMemberData(newId);
}, { immediate: true });

onMounted(() => {
  fetchMemberData(props.memberId);
});
</script>