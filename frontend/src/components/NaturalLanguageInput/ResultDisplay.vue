<template>
  <v-card class="pa-4" v-if="generatedData">
    <v-card-title class="text-h5">Kết quả phân tích</v-card-title>
    <v-card-subtitle>Loại dữ liệu: {{ generatedData.dataType }}</v-card-subtitle>
    <v-card-text>
      <div v-if="generatedData.families.length > 0">
        <h3 class="text-h6 mb-2">{{ t('naturalLanguageInput.families') }}</h3>
        <v-expansion-panels>
          <GeneratedEntityEditor
            v-for="(family, index) in generatedData.families"
            :key="`family-${index}`"
            :entity="family"
            :index="index"
            @update:entity="updateEntity"
            @remove:entity="removeEntity"
          />
        </v-expansion-panels>
      </div>

      <div v-if="generatedData.members.length > 0">
        <h3 class="text-h6 mb-2">{{ t('naturalLanguageInput.members') }}</h3>
        <v-expansion-panels>
          <GeneratedEntityEditor
            v-for="(member, index) in generatedData.members"
            :key="`member-${index}`"
            :entity="member"
            :index="index"
            @update:entity="updateEntity"
            @remove:entity="removeEntity"
          />
        </v-expansion-panels>
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

const updateEntity = ({ index, entity, type }: { index: number; entity: Family | Member; type: 'Family' | 'Member' }) => {
  if (!naturalLanguageInputStore.generatedData) return;

  if (type === 'Family') {
    naturalLanguageInputStore.generatedData.families[index] = entity as Family;
  } else if (type === 'Member') {
    naturalLanguageInputStore.generatedData.members[index] = entity as Member;
  }
};

const removeEntity = ({ index, type }: { index: number; type: 'Family' | 'Member' }) => {
  if (!naturalLanguageInputStore.generatedData) return;

  if (type === 'Family') {
    naturalLanguageInputStore.generatedData.families.splice(index, 1);
  } else if (type === 'Member') {
    naturalLanguageInputStore.generatedData.members.splice(index, 1);
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