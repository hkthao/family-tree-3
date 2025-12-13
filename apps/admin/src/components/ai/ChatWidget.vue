<template>
  <v-navigation-drawer v-model="chatOpen" location="right" temporary width="400" class="chat-window">
    <v-card flat class="fill-height d-flex flex-column">
      <v-card-text class="chat-messages-container pa-0">
        <v-btn class="btn-close" variant="text" density="compact" icon @click="toggleChat">
          <v-icon>mdi-close</v-icon>
        </v-btn>
        <p>Chat feature removed.</p>
      </v-card-text>
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useAuthStore } from '@/stores'; // Only AuthStore is needed for user context if any
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const chatOpen = ref(false);
const authStore = useAuthStore();

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update:modelValue']);

watch(() => props.modelValue, (newVal) => {
  chatOpen.value = newVal;
});

const toggleChat = () => {
  chatOpen.value = !chatOpen.value;
  emit('update:modelValue', chatOpen.value);
};
</script>

<style scoped>
.chat-window {
  width: 380px;
  height: fit-content !important;
  display: flex;
  flex-direction: column;
  border-radius: 8px;
  overflow: hidden;
  position: relative;
}

.btn-close {
  position: absolute;
  z-index: 10;
  top: 15px;
  right: 15px;
  color: rgb(var(--v-theme-primary));
}
</style>