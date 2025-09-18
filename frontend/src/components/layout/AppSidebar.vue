<template>
  <v-navigation-drawer v-model="drawer" app>
    <v-list-item class="pa-3">
      <v-list-item-title class="text-h5 font-weight-bold">
        <v-icon icon="mdi-family-tree" size="large" color="primary"></v-icon>
        Family Tree
      </v-list-item-title>
    </v-list-item>
    <v-divider></v-divider>

    <v-list nav>
      <v-list-subheader class="mt-4">USER FEATURES</v-list-subheader>
      <v-list-item
        v-for="(item, i) in userMenuItems"
        :key="i"
        :to="item.to"
        :value="item.title"
        link
      >
        <template v-slot:prepend>
          <v-icon :icon="item.icon"></v-icon>
        </template>
        <template v-slot:title>
          <span class="text-body-1">{{ item.title }}</span>
        </template>
        <template v-if="item.badge" v-slot:append>
          <v-badge color="primary" :content="item.badge" inline></v-badge>
        </template>
      </v-list-item>

      <v-list-subheader class="mt-4">ADMIN FEATURES</v-list-subheader>
      <v-list-item
        v-for="(item, i) in adminMenuItems"
        :key="i"
        :to="item.to"
        :value="item.title"
        link
      >
        <template v-slot:prepend>
          <v-icon :icon="item.icon"></v-icon>
        </template>
        <template v-slot:title>
          <span class="text-body-1">{{ item.title }}</span>
        </template>
        <template v-if="item.badge" v-slot:append>
          <v-badge color="primary" :content="item.badge" inline></v-badge>
        </template>
      </v-list-item>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref } from 'vue';

interface MenuItem {
  title: string;
  icon: string;
  to: string;
  badge?: string;
}

const drawer = ref(true); // Default to open

const userMenuItems: MenuItem[] = [
  { title: 'Dashboards', icon: 'mdi-view-dashboard', to: '/' },
  { title: 'Quản lý Gia đình', icon: 'mdi-account-group', to: '/family-management' },
  { title: 'Quản lý Thành viên', icon: 'mdi-account-multiple', to: '/members' },
  { title: 'Quan hệ', icon: 'mdi-link-variant', to: '/relationships' },
  { title: 'Cây Gia phả', icon: 'mdi-family-tree', to: '/tree' },
];

const adminMenuItems: MenuItem[] = [
  // Add admin-specific menu items here
  // Example: { title: 'User Management', icon: 'mdi-account-cog', to: '/admin/users' },
];
</script>
