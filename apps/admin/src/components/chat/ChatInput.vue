<template>
  <v-textarea
    ref="textareaRef"
    no-resize
    :auto-grow="false"
    counter
    :rows="2"
    :model-value="modelValue"
    @update:model-value="updateModelValue"
    :placeholder="placeholder"
    variant="outlined"
    @keyup.enter="handleEnterKey"
    :disabled="disabled"
  >
    <template v-slot:prepend-inner>
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn icon flat v-bind="props" :disabled="disabled" class="mr-2">
            <v-icon>mdi-plus</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item @click="triggerFileInput">
            <v-list-item-title>{{ t('chatInput.menu.addImagePdf') }}</v-list-item-title>
          </v-list-item>
          <v-list-item @click="getCurrentLocation">
            <v-list-item-title>{{ t('chatInput.menu.getCurrentLocation') }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
    </template>
    <template v-slot:append-inner>
      <v-btn icon flat :disabled="disabled || !modelValue?.trim()" @click="sendMessage">
        <v-icon>mdi-send</v-icon>
      </v-btn>
    </template>
  </v-textarea>
  <input type="file" ref="fileInput" @change="handleFileChange" accept="image/*,application/pdf" style="display: none;">
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useChatInput } from '@/composables/chat/useChatInput';

const props = defineProps({
  modelValue: String,
  placeholder: String,
  disabled: Boolean,
  loading: Boolean,
});

const emit = defineEmits([
  'update:modelValue',
  'sendMessage',
  'addAttachment', // Keep this for now, might be removed later if addImagePdf handles everything
  'addLocation',
]);

const { t } = useI18n();
const textareaRef = ref<HTMLTextAreaElement | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);

const { updateModelValue, handleEnterKey, sendMessage, addImagePdf, getCurrentLocation } = useChatInput(props, emit);

const triggerFileInput = () => {
  fileInput.value?.click();
};

const handleFileChange = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    const file = target.files[0];
    await addImagePdf(file);
    target.value = ''; // Clear the input so the same file can be selected again
  }
};
</script>

<style scoped>
/* Add any specific styles for ChatInput if needed */
</style>
