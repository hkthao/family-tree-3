<template>
  <v-container>
    <NLEditorInput :loading="naturalLanguageStore.loading" @parse-content="parseContent" />

    <ParsedDataList
      :parsed-result="naturalLanguageStore.parsedData"
      @delete-member="handleDeleteMember"
      @delete-event="handleDeleteEvent"
    />
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import ParsedDataList from '@/components/natural-language-input/ParsedDataList.vue';
import NLEditorInput from '@/components/natural-language-input/NLEditorInput.vue';
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store';

const props = defineProps<{
  familyId: string;
}>();

const naturalLanguageStore = useNaturalLanguageStore();

const parseContent = async (content: string | null) => {
  if (!content) {
    naturalLanguageStore.parsedData = null;
    return;
  }

  naturalLanguageStore.familyId = props.familyId;
  naturalLanguageStore.setInput(content);
  await naturalLanguageStore.analyzeContent();
};

const handleDeleteMember = (index: number) => {
  if (naturalLanguageStore.parsedData) {
    naturalLanguageStore.parsedData.members.splice(index, 1);
  }
};

const handleDeleteEvent = (index: number) => {
  if (naturalLanguageStore.parsedData) {
    naturalLanguageStore.parsedData.events.splice(index, 1);
  }
};

onMounted(() => {
  naturalLanguageStore.familyId = props.familyId;
});
</script>

