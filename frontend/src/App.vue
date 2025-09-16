<template>
  <v-app>
    <v-navigation-drawer app>
      <v-list-item title="Family Tree" subtitle="Menu"></v-list-item>
      <v-divider></v-divider>
      <v-list-item link to="/" title="Dashboard"></v-list-item>
      <v-list-item link to="/families" title="Quản lý Gia đình"></v-list-item>
      <v-list-item link to="/members" title="Quản lý Thành viên"></v-list-item>
      <v-list-item link to="/relationships" title="Quan hệ"></v-list-item>
      <v-list-item link to="/tree" title="Cây Gia phả"></v-list-item>
    </v-navigation-drawer>

    <v-app-bar app>
      <v-app-bar-title>Family Tree Project</v-app-bar-title>
    </v-app-bar>

    <v-main>
      <v-container fluid>
        <router-view></router-view>
      </v-container>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
//
</script>