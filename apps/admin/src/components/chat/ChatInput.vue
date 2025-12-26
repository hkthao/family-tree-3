<template>
  <div class="w-100">
    <div class="d-flex flex-wrap mb-2">
      <v-chip
        v-if="selectedLocation"
        class="mb-2 mr-2"
        closable
        color="primary"
        prepend-icon="mdi-map-marker"
        @update:modelValue="clearSelectedLocation"
        @click="openMapPicker(selectedLocation)"
      >
        {{ selectedLocation.source === 'current' ? t('chatInput.locationChipCurrent') : t('chatInput.locationChipMap') }}
      </v-chip>
      <v-chip
        v-for="file in uploadedFiles"
        :key="file.id"
        class="mb-2 mr-2"
        closable
        color="info"
        prepend-icon="mdi-file"
        @update:modelValue="removeUploadedFile(file.id)"
        @click="openFileInNewTab(file.url)"
      >
        {{ file.name }}
      </v-chip>
    </div>
    <v-textarea
      ref="textareaRef"
      no-resize
      :auto-grow="true"
      :max-rows="5"
      counter
      maxlength="1500"
      :rules="chatInputRules.chatInput"
      :rows="2"
      :model-value="modelValue"
      @update:model-value="updateModelValue"
      :placeholder="placeholder"
      variant="outlined"
      @keyup.enter="handleEnterKey"
      :disabled="disabled || isUploadingFile"
    >
      <template v-slot:prepend-inner>
        <v-menu>
          <template v-slot:activator="{ props }">
            <v-btn
              icon
              flat
              v-bind="props"
              :disabled="disabled || isUploadingFile || uploadedFiles.length >= MAX_FILE_COUNT"
              variant="text"
              class="mr-2"
            >
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
            <v-list-item @click="() => openMapPicker()">
              <v-list-item-title>{{ t('chatInput.menu.selectFromMap') }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </template>
      <template v-slot:append-inner>
        <v-btn icon flat :disabled="disabled || !modelValue?.trim() && uploadedFiles.length === 0" @click="sendMessage">
          <v-icon>mdi-send</v-icon>
        </v-btn>
      </template>
    </v-textarea>
    <input type="file" ref="fileInput" @change="handleFileChange" accept="image/*,application/pdf" style="display: none;">
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useChatInput } from '@/composables/chat/useChatInput'; // Import UploadedFile type
import { useChatInputRules } from '@/validations/chat.validation';

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

const MAX_FILE_COUNT = 3; // Re-declare for template use

const {
  updateModelValue,
  handleEnterKey,
  sendMessage,
  addImagePdf,
  getCurrentLocation,
  openMapPicker,
  clearSelectedLocation,
  selectedLocation,
  uploadedFiles, // Destructure new return values
  removeUploadedFile,
  isUploadingFile,
} = useChatInput(props, emit);

// Validation
const chatInputState = reactive({
  chatInput: props.modelValue,
});

const { rules: chatInputRules } = useChatInputRules(chatInputState);

// Watch for changes in modelValue and update chatInputState
watch(() => props.modelValue, (newValue) => {
  chatInputState.chatInput = newValue;
});

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

const openFileInNewTab = (url: string) => {
  window.open(url, '_blank');
};
</script>

<style scoped>
/* Add any specific styles for ChatInput if needed */
</style>
