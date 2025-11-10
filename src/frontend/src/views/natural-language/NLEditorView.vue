<template>
  <v-container>
    <v-card>
      <v-card-title>{{ t('naturalLanguage.editor.title') }}</v-card-title>
      <v-card-text>
        <editor-content :editor="editor" />
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="parseContent">{{ t('naturalLanguage.editor.parseButton') }}</v-btn>
      </v-card-actions>
    </v-card>

    <v-card v-if="parsedResult" class="mt-4">
      <v-card-title>{{ t('naturalLanguage.editor.parsedResultTitle') }}</v-card-title>
      <v-card-text>
        <pre>{{ parsedResult }}</pre>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { useEditor, EditorContent } from '@tiptap/vue-3';
import StarterKit from '@tiptap/starter-kit';
import Mention from '@tiptap/extension-mention';
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';

const { t } = useI18n();
const memberStore = useMemberStore();
const parsedResult = ref<any>(null);

const editor = useEditor({
  content: `<p>Type here and use '@' to mention people. For example: '@John Doe is the son of @Jane Doe'.</p>`,
  extensions: [
    StarterKit,
    Mention.configure({
      HTMLAttributes: {
        class: 'mention',
      },
      suggestion: {
        items: async ({ query }) => {
          await memberStore.searchMembers(query); // Assuming searchMembers action exists
          return memberStore.list.items;
        },
        render: () => {
          // This part needs a suggestion list component, which I'll skip for now
          // to keep the initial implementation simple.
          // A proper implementation would use a floating popup with a list of members.
          return {
            onStart: () => {},
            onUpdate: () => {},
            onExit: () => {},
            onKeyDown: () => false,
          };
        },
      },
    }),
  ],
});

const parseContent = () => {
  const content = editor.value?.getJSON();
  // In a real implementation, this would be sent to the backend.
  // For now, we'll just display the JSON content.
  parsedResult.value = JSON.stringify(content, null, 2);
};

onMounted(() => {
  //
});

onBeforeUnmount(() => {
  editor.value?.destroy();
});
</script>

<style>
.mention {
  background-color: #e0e0e0;
  border-radius: 4px;
  padding: 2px 4px;
}
</style>
