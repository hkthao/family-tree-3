<template>
  <v-chip v-if="memberDisplayData" size="small" class="ma-1">
    <v-avatar start>
      <v-img :src="memberDisplayData.avatarUrl" :alt="memberDisplayData.fullName"></v-img>
    </v-avatar>
    {{ memberDisplayData.fullName }}
  </v-chip>
  <span v-else></span>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';

const props = defineProps<{
  fullName?: string;
  avatarUrl?: string;
  gender?: string; // Add gender prop for default avatar logic
}>();

const getMemberAvatar = (member: { avatarUrl?: string; gender?: string }) => {
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

const memberDisplayData = computed(() => {
  if (props.fullName) {
    return {
      fullName: props.fullName,
      avatarUrl: getMemberAvatar({ avatarUrl: props.avatarUrl, gender: props.gender }),
    };
  }
  return null;
});
</script>