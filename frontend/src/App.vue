<template>
  <v-app>
    <AppSidebar v-model="drawer" />
    <TopBar @toggle-drawer="drawer = !drawer" />

    <v-main>
      <v-container fluid>
        <router-view></router-view>
      </v-container>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import AppSidebar from './components/layout/AppSidebar.vue';
import TopBar from './components/layout/TopBar.vue';

const drawer = ref(true);
</script>