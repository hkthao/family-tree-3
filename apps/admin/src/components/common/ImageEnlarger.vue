<template>
  <v-dialog v-model="dialog" max-width="800">
    <template v-slot:activator="{ props }">
      <v-img v-bind="props" :src="src" :alt="alt" class="enlarge-image-thumbnail" @click="openDialog"
        :height="thumbnailHeight" :width="thumbnailWidth" cover rounded>
      </v-img>
    </template>
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span class="headline">{{ alt || 'Image Preview' }}</span>
        <v-btn icon @click="closeDialog" variant="text">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-title>
      <v-card-text style="max-height: 80vh; overflow-y: auto;">
        <v-img :src="src" :alt="alt" contain rounded></v-img>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';

defineProps({
  src: {
    type: String,
    required: true,
  },
  alt: {
    type: String,
    default: 'Image',
  },
  thumbnailHeight: {
    type: [String, Number],
    default: 50,
  },
  thumbnailWidth: {
    type: [String, Number],
    default: 50,
  },
});

const dialog = ref(false);

const openDialog = () => {
  dialog.value = true;
};

const closeDialog = () => {
  dialog.value = false;
};
</script>

<style scoped>
.enlarge-image-thumbnail {
  cursor: pointer;
}
</style>
