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
          <v-list-item @click="addImagePdf">
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
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps({
  modelValue: String,
  placeholder: String,
  disabled: Boolean,
  loading: Boolean,
});

const emit = defineEmits([
  'update:modelValue',
  'sendMessage',
  'addAttachment',
  'addLocation',
]);

const { t } = useI18n();
const textareaRef = ref<HTMLTextAreaElement | null>(null);

const updateModelValue = (value: string) => {
  emit('update:modelValue', value);
};

const handleEnterKey = (event: KeyboardEvent) => {
  if (!event.shiftKey) {
    event.preventDefault(); // Prevent new line
    sendMessage();
  }
};

const sendMessage = () => {
  if (props.modelValue?.trim() && !props.disabled) {
    emit('sendMessage');
  }
};

const addImagePdf = () => {
  console.log('Add Image/PDF clicked');
  // Placeholder for future implementation
  emit('addAttachment', 'image'); // Default to image for now, can be expanded
};

const getCurrentLocation = () => {
  console.log('Get Current Location clicked');
  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      (position) => {
        const { latitude, longitude } = position.coords;
        console.log(`Latitude: ${latitude}, Longitude: ${longitude}`);
        // For now, just append to message, later can be a structured input
        const locationText = `Location: Latitude ${latitude}, Longitude ${longitude}`;
        emit('addLocation', { latitude, longitude });
        // Optionally, append to the current message
        emit('update:modelValue', `${props.modelValue}\n${locationText}`);
        // You might want to implement reverse geocoding here to get an address
      },
      (error) => {
        console.error('Error getting location:', error);
        // Inform user about error
        // alert(t('chatInput.errors.locationAccessDenied'));
      }
    );
  } else {
    console.error('Geolocation is not supported by this browser.');
    // Inform user about lack of support
    // alert(t('chatInput.errors.geolocationNotSupported'));
  }
};
</script>

<style scoped>
/* Add any specific styles for ChatInput if needed */
</style>
