<template>
  <div class="flex flex-col min-h-screen bg-gray-50 text-gray-800 font-sans" :style="{ backgroundImage: 'url(/images/background-texture.png)' }">
    <Navbar />
    <main class="flex-grow container mx-auto px-4 py-8 max-w-screen-xl">
      <slot />
    </main>
    <Footer />
  </div>
</template>

<script setup lang="ts">
import Navbar from './Navbar.vue';
import Footer from './Footer.vue';

// Ensure the background-texture.png exists in public/images/
// For now, it's a placeholder.
</script>

<style scoped>
/* Base layout styles can be further customized here if needed */
</style>
