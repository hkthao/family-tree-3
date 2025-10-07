<template>
  <v-card class="pa-4" elevation="2">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-family-tree</v-icon>
      <span class="ml-2">Tổng quan cây gia phả</span>
    </v-card-title>
    <v-card-text>
      <div class="text-center py-4">
        <v-icon size="64" color="grey lighten-1">mdi-graph</v-icon>
        <p class="mt-3 text-subtitle-1 text-grey">Bản xem trước biểu đồ cây gia phả sẽ hiển thị tại đây.</p>
        <v-btn color="primary" variant="outlined" class="mt-3">Xem toàn bộ cây</v-btn>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
// No specific logic for this placeholder component yet
</script>

<style scoped>
/* Add any specific styles for FamilyTreeOverview here */
</style>