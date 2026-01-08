<template>
  <v-navigation-drawer app>
    <v-list-item class="pa-4">
      <AppNameDisplay />
    </v-list-item>

    <v-divider></v-divider>

    <v-list nav>
      <template v-for="(section, i) in filteredMenu" :key="i">
        <VListSubheader v-if="section.titleKey">{{ $t(section.titleKey) }}</VListSubheader>
        <template v-for="(item, j) in section.items" :key="j">
          <v-list-group v-if="item.children && item.children.length > 0" :value="item.titleKey">
            <template v-slot:activator="{ props }">
              <v-list-item
                v-bind="props"
                :prepend-icon="item.icon"
                :title="$t(item.titleKey)"
                :id="item.to === '/dashboard' ? 'dashboard-link' : undefined"
              ></v-list-item>
            </template>
            <v-list-item
              v-for="(child, k) in item.children"
              :key="k"
              :to="child.to"
              :prepend-icon="child.icon"
              :title="$t(child.titleKey)"
              active-class="active-item"
              :exact="child.exact"
            ></v-list-item>
          </v-list-group>
          <v-list-item
            v-else
            :to="item.to && !item.href ? item.to : undefined"
            :href="item.href"
            :target="item.target"
            :prepend-icon="item.icon"
            :title="$t(item.titleKey)"
            active-class="active-item"
            :exact="item.exact && !item.href"
            :id="item.to === '/dashboard' ? 'dashboard-link' : undefined"
          ></v-list-item>
        </template>
      </template>
    </v-list>

    <template v-slot:append>
      <div class="pa-4 text-center text-caption">
        Version: {{ appVersion }}
      </div>
    </template>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { VListSubheader } from 'vuetify/components';
import { AppNameDisplay } from '@/components/common';
import { useAuthStore } from '@/stores/auth.store';
import { useSidebarMenu } from '@/composables/layout/useSidebarMenu'; // NEW

const appVersion = ref('N/A');
const authStore = useAuthStore();

const { filteredMenu } = useSidebarMenu(computed(() => authStore.user?.roles || []));

onMounted(async () => {
  try {
    const response = await fetch('/version.json');
    if (response.ok) {
      const data = await response.json();
      appVersion.value = data.version;
    } else {
      console.error('Failed to fetch version.json:', response.statusText);
    }
  } catch (error) {
    console.error('Error fetching version.json:', error);
  }
});
</script>

<style scoped>
.active-item {
  background-color: rgba(var(--v-theme-primary), 0.1);
  color: rgb(var(--v-theme-primary));
  border-radius: 8px;
}

.active-item .v-list-item-title {
  font-weight: bold;
}
</style>
