<template>
  <div class="d-flex justify-center">
    <AvatarDisplay :src="getAvatarUrl(member.avatarUrl, member.gender)" :gender="member.gender" :size="size" />
  </div>
</template>

<script setup lang="ts">
import { getAvatarUrl } from '@/utils/avatar.utils'; // NEW
import type { MemberDto } from '@/types';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue'; // Common avatar component

interface Props {
  member: MemberDto;
  size?: number; // Optional size prop
}

withDefaults(defineProps<Props>(), {
  size: 36, // Default size
});
</script>