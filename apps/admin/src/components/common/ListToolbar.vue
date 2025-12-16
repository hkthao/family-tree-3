<template>
  <v-toolbar flat>
    <v-toolbar-title>{{ title }}</v-toolbar-title>
    <v-spacer></v-spacer>

    <v-btn
      color="primary"
      icon
      @click="emit('create')"
      :data-testid="createButtonTestId"
      :aria-label="createButtonTooltip"
    >
      <v-tooltip :text="createButtonTooltip">
        <template v-slot:activator="{ props }">
          <v-icon v-bind="props">mdi-plus</v-icon>
        </template>
      </v-tooltip>
    </v-btn>

  </v-toolbar>
</template>

<script setup lang="ts">


const { title, createButtonTooltip, createButtonTestId } = defineProps<{
  title: string;
  createButtonTooltip: string;
  createButtonTestId: string;
}>();

const emit = defineEmits(['create']);
</script>

<style scoped></style>
