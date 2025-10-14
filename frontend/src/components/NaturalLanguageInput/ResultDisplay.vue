<template>
  <v-card class="pa-4" v-if="generatedData">
    <v-card-title class="text-h5">Kết quả phân tích</v-card-title>
    <v-card-subtitle>Loại dữ liệu: {{ generatedData.dataType }}</v-card-subtitle>
    <v-card-text>
      <v-expansion-panels class="mb-4">
        <v-expansion-panel title="Xem JSON chi tiết">
          <v-expansion-panel-text>
            <pre>{{ formattedJson }}</pre>
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>

      <div v-if="generatedData.families.length > 0">
        <h3 class="text-h6 mb-2">{{ t('naturalLanguageInput.families') }}</h3>
        <GeneratedEntityEditor
          v-for="(family, index) in generatedData.families"
          :key="`family-${index}`"
          :entity="family"
          :index="index"
          @update:entity="updateEntity"
        />
      </div>

      <div v-if="generatedData.members.length > 0">
        <h3 class="text-h6 mb-2">{{ t('naturalLanguageInput.members') }}</h3>
        <GeneratedEntityEditor
          v-for="(member, index) in generatedData.members"
          :key="`member-${index}`"
          :entity="member"
          :index="index"
          @update:entity="updateEntity"
        />
      </div>

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
import { useI18n } from 'vue-i18n';
import GeneratedEntityEditor from './GeneratedEntityEditor.vue';
import type { Family, Member } from '@/types';

const naturalLanguageInputStore = useNaturalLanguageInputStore();
const { t } = useI18n();

const generatedData = computed(() => naturalLanguageInputStore.generatedData);
const isLoading = computed(() => naturalLanguageInputStore.isLoading);

const formattedJson = computed(() => {
  if (generatedData.value) {
    const dataToDisplay: any = {};
    if (generatedData.value.families.length > 0) {
      dataToDisplay.families = generatedData.value.families;
    }
    if (generatedData.value.members.length > 0) {
      dataToDisplay.members = generatedData.value.members;
    }
    return JSON.stringify(dataToDisplay, null, 2);
  }
  return '';
});

const updateEntity = ({ index, entity, type }: { index: number; entity: Family | Member; type: 'Family' | 'Member' }) => {
  if (!naturalLanguageInputStore.generatedData) return;

  if (type === 'Family') {
    naturalLanguageInputStore.generatedData.families[index] = entity as Family;
  } else if (type === 'Member') {
    naturalLanguageInputStore.generatedData.members[index] = entity as Member;
  }
};

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