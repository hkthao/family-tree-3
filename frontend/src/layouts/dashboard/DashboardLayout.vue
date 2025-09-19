<template>
  <v-app>
    <Sidebar v-model="drawer" />
    <TopBar @toggle-drawer="drawer = !drawer" />
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { Sidebar, TopBar } from '@/components/layout';

const drawer = ref(true);
</script>
