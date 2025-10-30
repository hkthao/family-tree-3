<template>
  <v-dialog :model-value="modelValue" persistent max-width="400">
    <v-card>
      <v-card-title class="headline">{{ title || $t('confirmDelete.title') }}</v-card-title>
      <v-card-text>
        {{ message || $t('confirmDelete.message') }}
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" text @click="$emit('cancel')">{{ $t('common.cancel') }}</v-btn>
        <v-btn color="red-darken-1" text @click="$emit('confirm')" data-testid="confirm-delete-button">{{ $t('common.delete') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
interface Props {
  modelValue: boolean;
  title?: string;
  message?: string;
}

withDefaults(defineProps<Props>(), {
  modelValue: false,
  title: undefined,
  message: undefined,
});

interface Emits {
  (e: 'confirm'): void;
  (e: 'cancel'): void;
}
defineEmits<Emits>();
</script>