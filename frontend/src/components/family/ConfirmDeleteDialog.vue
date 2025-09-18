<template>
  <v-dialog :model-value="modelValue" persistent max-width="400">
    <v-card>
      <v-card-title class="headline">Confirm Deletion</v-card-title>
      <v-card-text>
        Bạn có chắc muốn xoá Family này không?
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" text @click="$emit('cancel')">Hủy</v-btn>
        <v-btn color="red-darken-1" text @click="$emit('confirm')">Xoá</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
interface Props {
  modelValue: boolean;
}

defineProps<Props>();
defineEmits(['confirm', 'cancel']);
</script>
