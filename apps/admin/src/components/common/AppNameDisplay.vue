<template>
  <span class="text-h6 font-weight-bold app-name-font">{{ appName }}</span>
</template>

<script setup lang="ts">
import { ref } from 'vue';

const appName = ref('Dòng Họ Việt'); // Centralized app name

// Optional: If you want to make it configurable via props
// const props = defineProps({
//   name: { type: String, default: 'Dòng Họ Việt' },
// });
// const appName = ref(props.name);
</script>

<style scoped>
.app-name-font {
  font-family: 'Dancing Script', cursive;
}
</style>
