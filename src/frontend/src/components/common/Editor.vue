<template>
  <div :id="holderId" class="editor-js-container"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch, nextTick, type PropType } from 'vue';
import EditorJS, { type OutputData } from '@editorjs/editorjs';
import Header from '@editorjs/header';
import List from '@editorjs/list';
import Paragraph from '@editorjs/paragraph';
import Quote from '@editorjs/quote';
import CodeTool from '@editorjs/code';
import Delimiter from '@editorjs/delimiter';
import InlineCode from '@editorjs/inline-code';
import Table from '@editorjs/table';

const props = defineProps({
  modelValue: { type: Object as PropType<OutputData>, default: () => ({ blocks: [] }) }, // Expect Editor.js data object
  readOnly: { type: Boolean, default: false },
  placeholder: { type: String, default: 'Enter content here...' },
});

const emit = defineEmits(['update:modelValue', 'ready', 'change']);

const editor = ref<EditorJS | null>(null);
const holderId = ref(`editor-js-holder-${Math.random().toString(36).substring(7)}`);
const internalChange = ref(false); // Flag to track internal changes

const tools = {
  header: Header,
  list: {
    class: List,
    config: {},
  },
  paragraph: {
    class: Paragraph as any,
    config: {
      inlineToolbar: true,
    },
  },
  quote: Quote,
  code: CodeTool,
  delimiter: Delimiter,
  inlineCode: InlineCode,
  table: Table,
};

const initializeEditor = async () => {
  if (editor.value) {
    await editor.value.destroy();
    editor.value = null;
  }

  editor.value = new EditorJS({
    holder: holderId.value,
    data: props.modelValue,
    readOnly: props.readOnly,
    placeholder: props.placeholder,
    tools: tools,
    onChange: async () => {
      internalChange.value = true; // Set flag before emitting
      const content = await editor.value?.save();
      emit('update:modelValue', content);
      emit('change', content);
    },
    onReady: () => {
      emit('ready');
    },
  });
};

onMounted(() => {
  nextTick(initializeEditor);
});

onBeforeUnmount(async () => {
  if (editor.value) {
    await editor.value.destroy();
    editor.value = null;
  }
});

watch(() => props.modelValue, async (newVal) => {
  if (!internalChange.value && editor.value && JSON.stringify(newVal) !== JSON.stringify(await editor.value.save())) {
    await editor.value.render(newVal);
  }
  internalChange.value = false; // Reset flag after processing external change
}, { deep: true });

watch(() => props.readOnly, (newVal) => {
  if (editor.value) {
    editor.value.readOnly.toggle(newVal);
  }
});

watch(() => props.placeholder, (newVal) => {
  // Editor.js does not have a direct method to update placeholder after initialization.
  // Re-initializing the editor is one way, but might be disruptive.
  // For now, we'll rely on the initial placeholder setting.
  // If dynamic placeholder updates are critical, a more complex solution (e.g., re-init) would be needed.
  console.warn('Editor.js placeholder cannot be updated dynamically after initialization without re-rendering the editor.');
});
</script>

<style>
.editor-js-container {
  border: 1px solid #ccc;
  border-radius: 4px;
  padding: 8px;
  min-height: 150px;
}

/* Adjustments for Editor.js specific classes */
.codex-editor__redactor {
  padding-bottom: 0 !important;
}
</style>