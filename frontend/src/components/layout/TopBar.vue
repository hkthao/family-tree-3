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

    <div class="mx-2">
            <UserMenu
              :current-user="currentUser"
              @navigate="handleNavigation"
              @logout="handleLogout"
              @open-settings="handleOpenSettings"
            />      
    </div>

  </v-app-bar>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useTheme } from 'vuetify';
import UserMenu from './UserMenu.vue';
import { useRouter } from 'vue-router';
import type { VTextField } from 'vuetify/components';

const theme = useTheme();
const searchField = ref<VTextField | null>(null);
const router = useRouter();

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
    searchField.value.focus();
  }
}

const handleNavigation = (route: string) => {
  router.push(route);
};

const handleLogout = () => {
  // Implement logout logic
  console.log('Logging out...');
};

const handleOpenSettings = () => {
  // Implement logic to open settings, e.g., navigate to settings page
  console.log('Opening settings...');
  router.push('/settings');
};

</script>
