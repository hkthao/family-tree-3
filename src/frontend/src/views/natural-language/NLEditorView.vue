<template>
  <v-container>
    <v-card>
      <v-progress-linear
        :active="naturalLanguageStore.loading"
        :indeterminate="naturalLanguageStore.loading"
        color="primary"
        absolute
        top
      ></v-progress-linear>
      <v-card-title>{{ t('naturalLanguage.editor.title') }}</v-card-title>
      <v-card-text class="pa-0"> <!-- Remove padding from v-card-text -->
        <div class="tiptap-editor">
          <v-toolbar density="compact">
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleBold().run()"
              :class="{ 'is-active': editor?.isActive('bold') }">
              <v-icon>mdi-format-bold</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleItalic().run()"
              :class="{ 'is-active': editor?.isActive('italic') }">
              <v-icon>mdi-format-italic</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleStrike().run()"
              :class="{ 'is-active': editor?.isActive('strike') }">
              <v-icon>mdi-format-strikethrough</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleCode().run()"
              :class="{ 'is-active': editor?.isActive('code') }">
              <v-icon>mdi-code-tags</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().setParagraph().run()"
              :class="{ 'is-active': editor?.isActive('paragraph') }">
              <v-icon>mdi-format-paragraph</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleHeading({ level: 1 }).run()"
              :class="{ 'is-active': editor?.isActive('heading', { level: 1 }) }">
              <v-icon>mdi-format-header-1</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleHeading({ level: 2 }).run()"
              :class="{ 'is-active': editor?.isActive('heading', { level: 2 }) }">
              <v-icon>mdi-format-header-2</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleHeading({ level: 3 }).run()"
              :class="{ 'is-active': editor?.isActive('heading', { level: 3 }) }">
              <v-icon>mdi-format-header-3</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleBulletList().run()"
              :class="{ 'is-active': editor?.isActive('bulletList') }">
              <v-icon>mdi-format-list-bulleted</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleOrderedList().run()"
              :class="{ 'is-active': editor?.isActive('orderedList') }">
              <v-icon>mdi-format-list-numbered</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleCodeBlock().run()"
              :class="{ 'is-active': editor?.isActive('codeBlock') }">
              <v-icon>mdi-code-braces</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().toggleBlockquote().run()"
              :class="{ 'is-active': editor?.isActive('blockquote') }">
              <v-icon>mdi-format-quote-open</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().setHorizontalRule().run()">
              <v-icon>mdi-minus</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().setHardBreak().run()">
              <v-icon>mdi-format-text-variant</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().undo().run()">
              <v-icon>mdi-undo</v-icon>
            </v-btn>
            <v-btn icon variant="text" size="small" @click="editor?.chain().focus().redo().run()">
              <v-icon>mdi-redo</v-icon>
            </v-btn>
          </v-toolbar>
          <editor-content :editor="editor" class="tiptap-editor-content" />
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="parseContent" :loading="naturalLanguageStore.loading" :disabled="naturalLanguageStore.loading">{{ t('naturalLanguage.editor.parseButton') }}</v-btn>
      </v-card-actions>
    </v-card>

    <v-card v-if="parsedResult" class="mt-4">
      <v-card-title>{{ t('naturalLanguage.editor.parsedResultTitle') }}</v-card-title>
      <v-card-text>
        <pre>{{ JSON.stringify(parsedResult, null, 2) }}</pre>
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
import { VueRenderer } from '@tiptap/vue-3';
import tippy, { type Instance as TippyInstance } from 'tippy.js';
import 'tippy.js/dist/tippy.css'; // For basic styling
import MentionList from '@/components/natural-language-input/MentionList.vue';
import { v4 as uuidv4 } from 'uuid'; // Import uuid for sessionId
import { useServices } from '@/composables/useServices'; // Import useServices
import type { AnalyzedDataDto } from '@/types/natural-language.d'; // Import AnalyzedDataDto
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store'; // Import naturalLanguage store

const { t } = useI18n();
const memberStore = useMemberStore();
const naturalLanguageStore = useNaturalLanguageStore(); // Use naturalLanguage store
const parsedResult = ref<AnalyzedDataDto | string | null>(null); // Update type to include AnalyzedDataDto
let popup: TippyInstance | null = null;

const { naturalLanguage: naturalLanguageService } = useServices(); // Inject naturalLanguage service

const editor = useEditor({
  content: `<p>Nguyễn Văn A sinh năm 1950, mất năm 2020. Ông là cha của Nguyễn Thị B và Nguyễn Văn C. Nguyễn Thị B sinh năm 1975, kết hôn với Trần Văn D. Họ có một người con tên là Trần Thị E. Nguyễn Văn C sinh năm 1980, chưa kết hôn.</p>`,
  extensions: [
    StarterKit,
    Mention.configure({
      HTMLAttributes: {
        class: 'mention',
      },
      renderHTML({ node }) {
        return ['span', {
          style: 'background-color: #1976D2; color: #FFFFFF; border-radius: 4px; padding: 2px 4px;',
          class: 'mention', // Keep the class for potential future use or other styling
        }, `@${node.attrs.label}`];
      },
      suggestion: {
        items: async ({ query }) => {
          await memberStore.searchMembers(query);
          return memberStore.list.items.map(item => ({
            id: item.code || '',
            label: `[${item.fullName || ''}](${item.code || ''})`,
          }));
        },
        render: () => {
          let component: VueRenderer | null = null;
          let mentionListOnKeyDown: ((props: { event: KeyboardEvent }) => boolean) | null = null;
          let popup: TippyInstance | null = null; // Changed to single instance

          return {
            onStart: props => {
              component = new VueRenderer(MentionList, {
                props: {
                  ...props,
                  onSelectCallback: (handler: { onKeyDown: (props: { event: KeyboardEvent }) => boolean }) => {
                    mentionListOnKeyDown = handler.onKeyDown;
                  },
                },
                editor: props.editor,
              });

              popup = tippy(document.querySelector('body') as HTMLElement, {
                getReferenceClientRect: props.clientRect as () => DOMRect,
                appendTo: () => document.body,
                content: component.element as HTMLElement,
                showOnCreate: true,
                interactive: true,
                trigger: 'manual',
                placement: 'bottom-start',
              });
            },
            onUpdate(props) {
              component?.updateProps(props);

              popup?.setProps({
                getReferenceClientRect: props.clientRect as () => DOMRect,
              });
            },
            onKeyDown(props) {
              if (mentionListOnKeyDown) {
                return mentionListOnKeyDown(props);
              }
              return false;
            },
            onExit() {
              if (popup) {
                popup.destroy();
                popup = null; // Set to null after destroying
              }
              component?.destroy();
            },
          };
        },
      },
    }),
  ],
});

const parseContent = async () => { // Make parseContent async
  const content = editor.value?.getText(); // Get plain text content
  if (!content) {
    parsedResult.value = 'Nội dung trống.';
    return;
  }

  const sessionId = uuidv4(); // Generate a new sessionId

  try {
    naturalLanguageStore.loading = true; // Set loading to true
    const result = await naturalLanguageService.analyzeContent(content, sessionId);

    if (result.ok) {
      // Assuming result.value is the JSON string, parse it
      parsedResult.value = JSON.parse(result.value);
    } else {
      parsedResult.value = `Lỗi: ${result.error?.message || 'Lỗi không xác định'}`;
    }
  } catch (error: any) {
    parsedResult.value = `Lỗi: ${error.response?.data?.title || error.message}`;
  } finally {
    naturalLanguageStore.loading = false; // Set loading to false
  }
};

onMounted(() => {
  //
});

onBeforeUnmount(() => {
  editor.value?.destroy();
});
</script>

<style lang="scss">
.tiptap-editor {
  border: none;
  border-radius: 4px;

  .tiptap-editor-content {
    min-height: 150px;
    padding: 12px;
    font-size: medium;
  }
}
.tiptap.ProseMirror{
  outline: none;
}

.tiptap-editor-content .mention {
  background-color:  rgb(105,108,255);
  color: #FFFFFF;  border-radius: 4px;
  padding: 2px 4px;
}

.v-btn.is-active {
  background-color: rgb(105,108,255);
}
</style>
