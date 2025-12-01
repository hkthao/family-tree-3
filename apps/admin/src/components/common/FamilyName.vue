<template>
  <v-chip v-if="familyDisplayData" size="small" class="ma-1">
    <v-avatar start>
      <v-img :src="familyDisplayData.avatarUrl" :alt="familyDisplayData.name"></v-img>
    </v-avatar>
    {{ familyDisplayData.name }}
  </v-chip>
  <span v-else></span>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils'; // Using the specific utility for family avatars

const props = defineProps<{
  name?: string;
  avatarUrl?: string;
}>();

const familyDisplayData = computed(() => {
  if (props.name) {
    return {
      name: props.name,
      avatarUrl: getFamilyAvatarUrl(props.avatarUrl),
    };
  }
  return null;
});
</script>