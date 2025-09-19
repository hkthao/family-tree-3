<template>
  <v-app-bar app flat class="border-bottom">
    <v-app-bar-nav-icon @click.stop="$emit('toggle-drawer')"></v-app-bar-nav-icon>
    <v-text-field
      density="comfortable"
      variant="solo"
      prepend-inner-icon="mdi-magnify"
      label="Search..."
      single-line
      hide-details
      class="mx-4"
      rounded
      @keydown.meta.k.prevent="focusSearch"
      ref="searchField"
    >
      <template v-slot:append-inner>
        <v-chip small class="text-caption">⌘K</v-chip>
      </template>
    </v-text-field>

    <v-spacer></v-spacer>

    <v-btn icon @click="toggleTheme">
      <v-icon>mdi-theme-light-dark</v-icon>
    </v-btn>

    <v-btn icon>
      <v-badge content="4" color="error">
        <v-icon>mdi-bell-outline</v-icon>
      </v-badge>
    </v-btn>

    <v-menu offset-y>
      <template v-slot:activator="{ props }">
        <v-avatar class="ml-4" size="36" v-bind="props">
          <v-img :src="currentUser.avatar || 'https://randomuser.me/api/portraits/men/85.jpg'"></v-img>
        </v-avatar>
      </template>
      <v-list>
        <v-list-item :to="{ name: 'UserProfile' }">
          <v-list-item-title>Profile</v-list-item-title>
        </v-list-item>
        <v-list-item :to="{ name: 'Settings' }">
          <v-list-item-title>Settings</v-list-item-title>
        </v-list-item>
        <v-divider></v-divider>
        <v-list-item @click="logout">
          <v-list-item-title>Logout</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-menu>
  </v-app-bar>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useTheme } from 'vuetify';

const theme = useTheme();
const searchField = ref(null);

defineProps({
  currentUser: {
    type: Object,
    required: true,
    default: () => ({ id: 'u1', name: 'John', roles: ['FamilyManager'], avatar: '' })
  }
});

defineEmits(['toggle-drawer']);

function toggleTheme() {
  theme.global.name.value = theme.global.current.value.dark ? 'light' : 'dark';
}

function focusSearch() {
  if (searchField.value) {
    (searchField.value as any).focus();
  }
}

function logout() {
  // Implement logout logic
  console.log('Logging out...');
}
</script>