<template>
  <v-app-bar app flat  >
    <v-app-bar-nav-icon
      @click.stop="$emit('toggle-drawer')"
    ></v-app-bar-nav-icon>
    <v-text-field
      density="comfortable"
      variant="solo"
      prepend-inner-icon="mdi-magnify"
      :label="t('topbar.search')"
      single-line
      hide-details
      class="mx-4"
      @keydown.meta.k.prevent="focusSearch"
      ref="searchField"
      rounded
      flat
    >
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
        @navigate="handleNavigation"
        @open-settings="handleOpenSettings"
      />
    </div>
  </v-app-bar>
</template>

<script setup lang="ts">
import { ref, type PropType } from 'vue';
import { useTheme } from 'vuetify';
import UserMenu from './UserMenu.vue';
import { useRouter } from 'vue-router';
import type { VTextField } from 'vuetify/components';
import type { User } from '@/types';
import { useI18n } from 'vue-i18n';
import { useUserSettingsStore } from '@/stores/userSettings.store';

const { t } = useI18n();
const theme = useTheme();
const searchField = ref<VTextField | null>(null);
const router = useRouter();
const userSettingsStore = useUserSettingsStore();

defineProps({
  currentUser: {
    type: Object as PropType<User | null>,
    required: false,
  },
});

defineEmits(['toggle-drawer']);

function toggleTheme() {
  const newTheme = theme.global.current.value.dark ? 'light' : 'dark';
  userSettingsStore.setTheme(newTheme);
  theme.global.name.value = newTheme;
}

function focusSearch() {
  if (searchField.value) {
    searchField.value.focus();
  }
}

const handleNavigation = (route: string) => {
  router.push(route);
};

const handleOpenSettings = () => {
  // Implement logic to open settings, e.g., navigate to settings page
  console.log('Opening settings...');
  router.push('/settings');
};
</script>
