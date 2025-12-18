<template>
  <div ref="novuInbox" class="novu-inbox-container"></div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useNovuInbox } from '@/composables';

const novuInbox = ref<HTMLElement | null>(null);

useNovuInbox(novuInbox);
</script>
<style lang="css" scoped>
.novu-inbox-container{
  min-width: 320px;
}
</style>
