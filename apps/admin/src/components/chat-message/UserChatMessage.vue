<template>
  <div class="d-flex align-center my-1 justify-end">
    <v-sheet class="ma-1 pa-2 text-wrap" color="primary" rounded="lg">
      <div class="message-content">
        {{ message.text }}
      </div>
    </v-sheet>
    <v-avatar cover class="ml-1" size="36">
      <v-img v-if="userProfile?.value?.avatar" :src="getAvatarUrl(userProfile.value.avatar, undefined)"
        :alt="userProfile.value.name || 'User'" />
      <v-icon v-else>mdi-account-circle</v-icon>
    </v-avatar>
  </div>
</template>

<script setup lang="ts">
import type { PropType, Ref } from 'vue'; // Use type import for PropType and Ref
import type { AiChatMessage } from '@/types/chat.d'; // Use type import
import { getAvatarUrl } from '@/utils/avatar.utils';
import type { UserProfile } from '@/types/user.d'; // Import UserProfile

defineProps({
  message: {
    type: Object as PropType<AiChatMessage>,
    required: true,
  },
  userProfile: {
    type: Object as PropType<Ref<UserProfile | null>>, // Use imported UserProfile
    default: null,
  },
});
</script>

<style scoped>
.message-content {
  white-space: pre-wrap;
  /* Preserves whitespace and wraps text */
  word-break: break-word;
  /* Ensures long words break to prevent overflow */
}
</style>