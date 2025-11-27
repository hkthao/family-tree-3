<template>
  <v-card flat>
    <v-card-text>
      <v-text-field
        v-model="editableDraft.title"
        :label="t('memory.storyEditor.title')"
        outlined
        class="mb-4"
      ></v-text-field>
      <v-textarea
        v-model="editableDraft.story"
        :label="t('memory.storyEditor.storyContent')"
        outlined
        rows="10"
        auto-grow
        class="mb-4"
      ></v-textarea>

      <v-expansion-panels flat class="mb-4">
        <v-expansion-panel :title="t('memory.storyEditor.details')">
          <v-expansion-panel-text>
            <v-combobox
              v-model="editableDraft.tags"
              :label="t('memory.storyEditor.tags')"
              multiple
              chips
              class="mb-4"
            ></v-combobox>
            <v-combobox
              v-model="editableDraft.keywords"
              :label="t('memory.storyEditor.keywords')"
              multiple
              chips
              class="mb-4"
            ></v-combobox>

            <h4 class="text-subtitle-1 mb-2">{{ t('memory.storyEditor.timeline') }}</h4>
            <v-card v-for="(item, index) in editableDraft.timeline" :key="index" class="mb-2 pa-2">
              <v-row align="center">
                <v-col cols="3">
                  <v-text-field
                    v-model.number="item.year"
                    :label="t('memory.storyEditor.year')"
                    type="number"
                    density="compact"
                    hide-details
                  ></v-text-field>
                </v-col>
                <v-col cols="8">
                  <v-text-field
                    v-model="item.event"
                    :label="t('memory.storyEditor.event')"
                    density="compact"
                    hide-details
                  ></v-text-field>
                </v-col>
                <v-col cols="1">
                  <v-btn icon flat small @click="removeTimelineEntry(index)">
                    <v-icon>mdi-delete</v-icon>
                  </v-btn>
                </v-col>
              </v-row>
            </v-card>
            <v-btn block color="secondary" @click="addTimelineEntry">
              {{ t('memory.storyEditor.addTimelineEntry') }}
            </v-btn>
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

interface TimelineEntry {
  year: number;
  event: string;
}

interface Draft {
  title: string;
  story: string;
  tags?: string[];
  keywords?: string[];
  timeline?: TimelineEntry[];
}

interface Props {
  draft: Draft;
}

const props = defineProps<Props>();
const emit = defineEmits(['update:draft']);
const { t } = useI18n();

const editableDraft = ref<Draft>({ ...props.draft });

watch(
  () => props.draft,
  (newDraft) => {
    editableDraft.value = { ...newDraft };
  },
  { deep: true }
);

watch(
  editableDraft,
  (newVal) => {
    emit('update:draft', newVal);
  },
  { deep: true }
);

const addTimelineEntry = () => {
  if (!editableDraft.value.timeline) {
    editableDraft.value.timeline = [];
  }
  editableDraft.value.timeline.push({ year: new Date().getFullYear(), event: '' });
};

const removeTimelineEntry = (index: number) => {
  if (editableDraft.value.timeline) {
    editableDraft.value.timeline.splice(index, 1);
  }
};
</script>

<style scoped>
/* Scoped styles for StoryEditor */
</style>
