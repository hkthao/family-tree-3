<template>
  <div>
    MemoryItem Edit View
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
  memoryItemId: {
    type: String as PropType<string>,
    required: true,
  },
});

</script>

<style scoped>
/* Scoped styles */
</style>
