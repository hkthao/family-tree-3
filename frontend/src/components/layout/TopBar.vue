<template>
  <v-app-bar app elevation="2">
    <v-app-bar-nav-icon @click="toggleDrawer"></v-app-bar-nav-icon>

    <v-spacer></v-spacer>

    <v-col cols="12" sm="6" md="4" lg="3">
      <v-text-field
        density="compact"
        variant="solo"
        label="Search..."
        append-inner-icon="mdi-magnify"
        single-line
        hide-details
      >
        <template v-slot:append-inner>
          <v-icon>mdi-magnify</v-icon>
          <v-chip size="small" class="ml-2">⌘K</v-chip>
        </template>
      </v-text-field>
    </v-col>

    <v-spacer></v-spacer>

    <v-btn icon @click="toggleTheme">
      <v-icon>{{ isDark ? 'mdi-weather-sunny' : 'mdi-weather-night' }}</v-icon>
    </v-btn>

    <v-btn icon class="mr-2">
      <v-badge content="5" color="error">
        <v-icon>mdi-bell-outline</v-icon>
      </v-badge>
    </v-btn>

    <v-avatar class="mr-2">
      <v-img src="https://randomuser.me/api/portraits/men/85.jpg"></v-img>
    </v-avatar>
  </v-app-bar>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useTheme } from 'vuetify';

const emit = defineEmits(['toggle-drawer']);

const theme = useTheme();
const isDark = ref(theme.global.current.value.dark);

const toggleTheme = () => {
  theme.global.name.value = theme.global.current.value.dark ? 'light' : 'dark';
  isDark.value = !isDark.value;
};

const toggleDrawer = () => {
  emit('toggle-drawer');
};
</script>