<template>
  <v-card class="pa-4" v-if="generatedData">
    <v-card-title class="text-h5">Kết quả phân tích</v-card-title>
    <v-card-subtitle>Loại dữ liệu: {{ generatedData.dataType }}</v-card-subtitle>
    <v-card-text>
      <v-expansion-panels>
        <v-expansion-panel title="Xem JSON chi tiết">
          <v-expansion-panel-text>
            <pre>{{ formattedJson }}</pre>
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="error" variant="text" @click="cancel">Hủy</v-btn>
      <v-btn color="success" :loading="isLoading" @click="confirm">Xác nhận & Lưu</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useNaturalLanguageInputStore } from '@/stores/naturalLanguageInput.store';

const naturalLanguageInputStore = useNaturalLanguageInputStore();

const generatedData = computed(() => naturalLanguageInputStore.generatedData);
const isLoading = computed(() => naturalLanguageInputStore.isLoading);

const formattedJson = computed(() => {
  if (generatedData.value) {
    if (generatedData.value.dataType === 'Family' && generatedData.value.family) {
      return JSON.stringify(generatedData.value.family, null, 2);
    } else if (generatedData.value.dataType === 'Member' && generatedData.value.member) {
      return JSON.stringify(generatedData.value.member, null, 2);
    }
  }
  return '';
});

const confirm = () => {
  naturalLanguageInputStore.saveData();
};

const cancel = () => {
  naturalLanguageInputStore.clearGeneratedData();
};
</script>

<style scoped>
pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  background-color: #f5f5f5;
  padding: 10px;
  border-radius: 4px;
}
</style>