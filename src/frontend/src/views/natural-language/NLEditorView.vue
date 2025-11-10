<template>
  <v-container>
    <NLEditorInput @parsed="handleParsedResult" />

    <ParsedDataList
      :parsed-result="parsedResult"
      @delete-member="handleDeleteMember"
      @delete-event="handleDeleteEvent"
    />
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import ParsedDataList from '@/components/natural-language-input/ParsedDataList.vue';
import NLEditorInput from '@/components/natural-language-input/NLEditorInput.vue';
import type { AnalyzedDataDto } from '@/types/natural-language.d'; // Import DTOs

const parsedResult = ref<AnalyzedDataDto | null>(null);

const handleParsedResult = (result: AnalyzedDataDto | null) => {
  parsedResult.value = result;
};

const handleDeleteMember = (index: number) => {
  if (parsedResult.value) {
    parsedResult.value.members.splice(index, 1);
  }
};

const handleDeleteEvent = (index: number) => {
  if (parsedResult.value) {
    parsedResult.value.events.splice(index, 1);
  }
};
</script>

