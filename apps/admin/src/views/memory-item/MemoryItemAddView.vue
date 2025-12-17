<template>
  <div>
    MemoryItem Add View
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
});

</script>

<style scoped>
/* Scoped styles */
</style>
