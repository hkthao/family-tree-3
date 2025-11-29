<template>
  <div class="d-flex justify-center">
    <AvatarDisplay :src="getAvatarUrl(member.avatarUrl, member.gender)" :gender="member.gender" :size="size" />
  </div>
</template>

<script setup lang="ts">
import { getAvatarUrl } from '@/utils/avatar.utils'; // NEW
import type { Member } from '@/types';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue'; // Common avatar component

interface Props {
  member: Member;
  size?: number; // Optional size prop
}

const props = withDefaults(defineProps<Props>(), {
  size: 36, // Default size
});
</script>