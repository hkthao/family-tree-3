<template>
  <v-card class="pa-4">
    <v-card-text>
      <v-textarea
        v-model="prompt"
        label="Nhập mô tả về gia đình hoặc thành viên (ví dụ: 'Tạo một gia đình tên Nguyễn ở Hà Nội' hoặc 'Thêm thành viên tên Trần Văn A, sinh năm 1990')"
        rows="3"
        variant="outlined"
        clearable
        :loading="isLoading"
        :disabled="isLoading"
        :hide-details="true"
      ></v-textarea>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn
        color="primary"
        :loading="isLoading"
        :disabled="!prompt.trim() || isLoading"
        @click="submitPrompt"
      >
        Gửi
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useNaturalLanguageInputStore } from '@/stores/naturalLanguageInput.store';

const naturalLanguageInputStore = useNaturalLanguageInputStore();
const prompt = ref(`Tạo một gia đình tên Nguyễn ở Hà Nội`);

const isLoading = naturalLanguageInputStore.isLoading; // Reactive loading state

const submitPrompt = () => {
  if (prompt.value.trim()) {
    naturalLanguageInputStore.generateData(prompt.value);
  }
};
</script>

<style scoped>
/* Add any specific styles here if needed */
</style>