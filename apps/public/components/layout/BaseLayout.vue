<script setup lang="ts">
import Navbar from './Navbar.vue';
import Footer from './Footer.vue';
</script>

<template>
  <div class="min-h-screen flex flex-col bg-gray-100 font-sans text-gray-900">
    <Navbar />
    <main class="flex-grow">
      <div class="relative isolate overflow-hidden pt-14">
        <img
          src="/images/background-texture.png"
          alt="Background texture"
          class="absolute inset-0 -z-10 h-full w-full object-cover opacity-50"
        />
        <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12">
          <slot />
        </div>
      </div>
    </main>
    <Footer />
  </div>
</template>
