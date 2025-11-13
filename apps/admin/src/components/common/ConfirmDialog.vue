<template>
  <v-dialog v-model="isDialogOpen" :max-width="dialogOptions.width || '500px'" persistent>
    <v-card>
      <v-card-title class="headline">{{ dialogOptions.title }}</v-card-title>
      <v-card-text>{{ dialogOptions.message }}</v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn :color="dialogOptions.cancelColor || 'grey'" text @click="cancel">{{ dialogOptions.cancelText }}</v-btn>
        <v-btn :color="dialogOptions.confirmColor || 'primary'" @click="confirm">{{ dialogOptions.confirmText }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { useConfirmDialog } from '@/composables/useConfirmDialog';

const { isDialogOpen, dialogOptions, confirm, cancel } = useConfirmDialog();
</script>