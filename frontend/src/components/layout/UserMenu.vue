<template>
  <v-menu
    offset-y
    :placement="placement"
    :close-on-content-click="false"
    v-model="menuOpen"
  >
    <template v-slot:activator="{ props: activatorProps }">
              <v-avatar
                size="36"
                class="cursor-pointer"
                :aria-label="$t('userMenu.ariaLabel')"
                aria-haspopup="true"
                :aria-expanded="menuOpen"
                v-bind="activatorProps"
              >
                <v-img v-if="currentUser.avatarUrl" :src="currentUser.avatarUrl"></v-img>
                <span v-else class="text-h6">{{ userInitials }}</span>
              </v-avatar>
    </template>

    <v-sheet class="user-menu-sheet rounded-lg elevation-3 pa-3">
      <!-- Header -->
      <div class="user-menu-header d-flex flex-column align-center">
        <v-avatar size="56" :color="currentUser.avatarUrl ? 'transparent' : 'primary'">
          <v-img v-if="currentUser.avatarUrl" :src="currentUser.avatarUrl"></v-img>
          <span v-else class="text-h5">{{ userInitials }}</span>
        </v-avatar>
        <!-- Placeholder for upload photo button -->
        <v-btn icon size="small"  class="mt-2">
          <v-icon>mdi-camera</v-icon>
          <v-tooltip activator="parent" location="bottom">{{ $t('userMenu.uploadPhoto') }}</v-tooltip>
        </v-btn>
        <div class="d-flex flex-column align-center mt-2">
          <span class="text-h6 font-weight-medium">{{ currentUser.name }}</span>
          <v-chip label size="small" color="primary" variant="tonal" class="mt-1">
            {{ $t('userMenu.role') }}: {{ currentUser.roles[0] }}
          </v-chip>
        </div>
      </div>

      <!-- Menu Items -->
      <v-list dense nav>
        <v-list-item
          v-for="item in menuItems"
          :key="item.key"
          :to="item.to"
          :prepend-icon="item.icon"
          @click="handleMenuItemClick(item.to)"
          class="user-menu-item"
        >
          <v-list-item-title>{{ $t(item.labelKey) }}</v-list-item-title>
          <v-tooltip activator="parent" location="right">{{ $t(item.labelKey) }}</v-tooltip>
        </v-list-item>
      </v-list>

      <v-divider class="my-2"></v-divider>

      <!-- Logout -->
      <v-list-item
        prepend-icon="mdi-logout"
        color="error"
        @click="confirmLogoutDialog = true"
        class="user-menu-item"
      >
        <v-list-item-title>{{ $t('userMenu.logout') }}</v-list-item-title>
        <v-tooltip activator="parent" location="right">{{ $t('userMenu.logout') }}</v-tooltip>
      </v-list-item>
    </v-sheet>
  </v-menu>

  <!-- Confirm Logout Dialog -->
  <v-dialog v-model="confirmLogoutDialog" max-width="350">
    <v-card>
      <v-card-title class="headline">{{ $t('userMenu.logout') }}</v-card-title>
      <v-card-text>{{ $t('userMenu.confirmLogout') }}</v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" text @click="confirmLogoutDialog = false">{{ $t('userMenu.cancel') }}</v-btn>
        <v-btn color="error" text @click="handleLogoutConfirm">{{ $t('userMenu.logout') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import type { User } from './UserMenu.types';
import { userMenuItems } from '@/data/userMenuItems';
import { ref, computed, watch } from 'vue';
const menuItems = userMenuItems;

const props = defineProps({
  currentUser: {
    type: Object as () => User,
    required: true,
  },
  placement: {
    type: String as () => 'bottom' | 'bottom-end' | 'bottom-start',
    default: 'bottom-end',
  },
});

const emit = defineEmits(['navigate', 'logout', 'openSettings']);

const menuOpen = ref(false);
const confirmLogoutDialog = ref(false);

const userInitials = computed(() => {
  if (props.currentUser.name) {
    const parts = props.currentUser.name.split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    } else if (parts.length === 1) {
      return parts[0][0].toUpperCase();
    }
  }
  return '';
});

const handleMenuItemClick = (route?: string) => {
  if (route) {
    emit('navigate', route);
  }
  menuOpen.value = false;
};

const handleLogoutConfirm = () => {
  confirmLogoutDialog.value = false;
  emit('logout');
};

// Accessibility: Focus management
watch(menuOpen, (newVal) => {
  if (newVal) {
    // Focus the first menu item when opened
    // This might require a ref on the v-list-item and nextTick
    // For now, just a placeholder comment
    console.log('Menu opened, focusing first item...');
  } else {
    // Return focus to activator when closed
    console.log('Menu closed, returning focus to activator...');
  }
});

// Global shortcut Ctrl/Cmd+U to open UserMenu (placeholder)
const handleGlobalKeyDown = (event: KeyboardEvent) => {
  if ((event.metaKey || event.ctrlKey) && event.key === 'u') {
    event.preventDefault();
    menuOpen.value = !menuOpen.value;
  }
};

// Attach global keydown listener
import { onMounted, onBeforeUnmount } from 'vue';
onMounted(() => {
  window.addEventListener('keydown', handleGlobalKeyDown);
});
onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleGlobalKeyDown);
});</script>

<style scoped>

.user-menu-header {
  display: flex;
  align-items: center;
  justify-content: center; /* Added for centering */
  gap: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid rgba(var(--v-border-color), 0.12);
  margin-bottom: 8px;
}
</style>
