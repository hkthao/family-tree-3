<template>
  <v-dialog :model-value="modelValue" persistent max-width="400">
    <v-card>
      <v-card-title class="headline">{{ $t('confirmDelete.title') }}</v-card-title>
      <v-card-text>
        {{ $t('confirmDelete.message') }}
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" text @click="$emit('cancel')">{{ $t('common.cancel') }}</v-btn>
        <v-btn color="red-darken-1" text @click="$emit('confirm')">{{ $t('common.delete') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">

import type { ConfirmDeleteDialogProps } from '@/types/family';

defineProps<ConfirmDeleteDialogProps>();
defineEmits(['confirm', 'cancel']);

</script>