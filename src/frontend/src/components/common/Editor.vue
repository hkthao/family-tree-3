<template>
  <VueEditorJS v-model="editorData" :read-only="readOnly" :placeholder="placeholder" :tools="tools" />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { VueEditorJS } from 'vue-editor-js';
import Header from '@editorjs/header';
import List from '@editorjs/list';
import Paragraph from '@editorjs/paragraph';
import Quote from '@editorjs/quote';
import CodeTool from '@editorjs/code';
import Delimiter from '@editorjs/delimiter';
import InlineCode from '@editorjs/inline-code';
import Table from '@editorjs/table';

const props = defineProps({
  modelValue: { type: Object, default: () => ({ blocks: [] }) }, // Expect Editor.js data object
  readOnly: { type: Boolean, default: false },
  placeholder: { type: String, default: 'Enter content here...' },
});

const emit = defineEmits(['update:modelValue']);

const tools = {
  header: Header,
  list: List,
  paragraph: {
    class: Paragraph,
    inlineToolbar: true,
  },
  quote: Quote,
  code: CodeTool,
  delimiter: Delimiter,
  inlineCode: InlineCode,
  table: Table,
};

// Internal ref to hold the Editor.js data object
const editorData = ref(props.modelValue);

// Watch for changes in editorData and emit update:modelValue
watch(editorData, (newVal) => {
  emit('update:modelValue', newVal);
}, { deep: true });

// Watch for external changes to modelValue and update editorData
watch(() => props.modelValue, (newVal) => {
  if (JSON.stringify(newVal) !== JSON.stringify(editorData.value)) {
    editorData.value = newVal;
  }
}, { deep: true });
</script>

<style>
.editor-js-container {
  border: 1px solid #ccc;
  border-radius: 4px;
  padding: 8px;
  min-height: 150px;
}

.codex-editor__redactor {
  padding-bottom: 0 !important;
}
</style>
