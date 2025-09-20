<template>
  <v-app>
    <Sidebar v-model="drawer" :current-user="currentUser" />
    <TopBar @toggle-drawer="drawer = !drawer" :current-user="currentUser" />
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { Sidebar, TopBar } from '@/components/layout';
import { mockUser } from '@/data/userMock';

const drawer = ref(true);
const currentUser = ref(mockUser);
</script>

<style scoped>
.v-application {
  background: #F5F5F9 !important;
}
</style>
