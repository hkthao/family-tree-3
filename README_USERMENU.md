# UserMenu Component Integration Guide

This guide explains how to integrate the `UserMenu` component into your application, specifically into the `TopBar.vue` component.

## 1. Import UserMenu

First, import the `UserMenu` component in your `TopBar.vue` script section:

```typescript
import UserMenu from '@/components/layout/UserMenu.vue';
```

## 2. Integrate into TopBar.vue Template

Place the `UserMenu` component in your `TopBar.vue` template, typically on the right side of the app bar:

```vue
<template>
  <v-app-bar app flat class="border-bottom">
    <!-- ... other top bar content ... -->

    <v-spacer></v-spacer>

    <!-- ... other top bar buttons ... -->

    <UserMenu
      :current-user="currentUser"
      :notifications-count="notificationsCount"
      @navigate="handleNavigation"
      @logout="handleLogout"
      @open-settings="handleOpenSettings"
    />
  </v-app-bar>
</template>
```

## 3. Define Props and Event Handlers in TopBar.vue Script

In your `TopBar.vue` script, define the `currentUser` prop and the `notificationsCount` prop. Implement the event handlers for `navigate`, `logout`, and `openSettings`.

```typescript
<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import UserMenu from '@/components/layout/UserMenu.vue';
import { mockUser } from '@/data/userMock'; // Example mock user

const router = useRouter();

// Example: Replace with your actual user data from a store or API
const currentUser = ref(mockUser);
const notificationsCount = ref(3); // Example count

const handleNavigation = (route: string) => {
  router.push(route);
};

const handleLogout = () => {
  // Implement your logout logic here
  console.log('User logged out!');
};

const handleOpenSettings = () => {
  // Implement logic to open settings, e.g., navigate to settings page
  console.log('Opening settings...');
  router.push('/settings');
};
</script>
```

## 4. Example `currentUser` Mock

For development and testing, you can use a mock user object. This is typically defined in `/data/userMock.ts`:

```typescript
// /data/userMock.ts
export const mockUser = {
  id: 'u1',
  name: 'John Doe',
  role: 'Admin',
  avatarUrl: 'https://randomuser.me/api/portraits/men/85.jpg',
  online: true,
};
```

## 5. Running Tests

To run the unit tests for `UserMenu.vue` (located in `/tests/UserMenu.spec.ts`):

```bash
npm run test:unit
```

This command will execute tests using Vitest.
