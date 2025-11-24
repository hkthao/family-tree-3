<template>
  <div class="d-flex justify-center">
    <AvatarDisplay :src="memberAvatarUrl" :gender="member.gender" :size="size" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { Member } from '@/types';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue'; // Common avatar component
import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';

interface Props {
  member: Member;
  size?: number; // Optional size prop
}

const props = withDefaults(defineProps<Props>(), {
  size: 36, // Default size
});

const memberAvatarUrl = computed(() => {
  if (props.member.avatarUrl) {
    return props.member.avatarUrl;
  }
  if (props.member.gender === 'Male') {
    return maleAvatar;
  }
  if (props.member.gender === 'Female') {
    return femaleAvatar;
  }
  return maleAvatar; // Fallback for 'Other' or undefined gender
});
</script>