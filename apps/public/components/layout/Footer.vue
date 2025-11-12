<template>
  <footer class="bg-gray-800 text-gray-300 py-8 mt-12">
    <div class="container mx-auto px-4 text-center">
      <p class="mb-4">
        Family Tree App - Nơi lưu giữ và kết nối những câu chuyện gia đình.
      </p>
      <div class="flex justify-center space-x-6 mb-4">
        <NuxtLink to="/legal/privacy" class="hover:text-white">Privacy Policy</NuxtLink>
        <NuxtLink to="/legal/terms" class="hover:text-white">Terms of Use</NuxtLink>
        <NuxtLink to="/legal/cookies" class="hover:text-white">Cookie Policy</NuxtLink>
      </div>
      <p>&copy; {{ currentYear }} Family Tree App. All rights reserved.</p>
    </div>
  </footer>
</template>

<script setup lang="ts">
const currentYear = new Date().getFullYear();
</script>

<style scoped>
/* Add any specific styles for Footer here if needed */
</style>
