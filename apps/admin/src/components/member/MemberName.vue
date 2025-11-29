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
import { getAvatarUrl } from '@/utils/avatar.utils'; // NEW

const props = defineProps<{
  fullName?: string;
  avatarUrl?: string;
  gender?: string; // Add gender prop for default avatar logic
}>();

const memberDisplayData = computed(() => {
  if (props.fullName) {
    return {
      fullName: props.fullName,
      avatarUrl: getAvatarUrl(props.avatarUrl, props.gender),
    };
  }
  return null;
});
</script>