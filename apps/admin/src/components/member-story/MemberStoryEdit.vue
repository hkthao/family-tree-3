<template>
  <v-card flat>
    <v-card-title class="d-flex align-center">
      <v-btn icon @click="emit('close')" variant="text">
        <v-icon>mdi-close</v-icon>
      </v-btn>
      <span class="text-h6">{{ t('memberStory.edit.title') }}</span>
      <v-spacer></v-spacer>
    </v-card-title>
    <v-card-text>
      <v-container v-if="loading">
        <v-row>
          <v-col cols="12" class="text-center">
            <v-progress-circular indeterminate color="primary"></v-progress-circular>
            <p class="mt-2">{{ t('memberStory.edit.loading') }}</p>
          </v-col>
        </v-row>
      </v-container>
      <v-container v-else-if="editableMemberStory">
        <v-row>
          <v-col cols="12">
            <v-img v-if="editableMemberStory.photoUrl" :src="editableMemberStory.photoUrl" max-height="200" contain class="mb-4"></v-img>
          </v-col>
        </v-row>
      </v-container>
      <v-container v-else>
        <v-alert type="error">{{ t('memberStory.edit.notFound') }}</v-alert>
      </v-container>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="saveMemberStory" :disabled="!editableMemberStory || savingMemberStory" :loading="savingMemberStory">
        {{ t('common.save') }}
      </v-btn>
      <v-btn color="secondary" @click="emit('close')" :disabled="savingMemberStory">
        {{ t('common.cancel') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store'; // Updated

interface Props {
  memberStoryId: string; // Updated
}
const props = defineProps<Props>();
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemberStoryStore(); // Updated
const editableMemberStory = ref<any | null>(null); // Updated
const loading = ref(false);
const savingMemberStory = ref(false); // Updated

  const loadMemberStory = async (id: string) => { // Updated
  loading.value = true;
  const memberStoryData = await memoryStore.getById(id); // Updated
  if (memberStoryData) {
    editableMemberStory.value = { ...memberStoryData }; // Updated
  } else {
    editableMemberStory.value = null; // Updated
  }
  loading.value = false;
};



const saveMemberStory = async () => { // Updated
  if (!editableMemberStory.value) return;

  savingMemberStory.value = true; // Updated
  const updatePayload = {
    id: props.memberStoryId, // Updated
    memberId: editableMemberStory.value.memberId, // Updated
    title: editableMemberStory.value.title, // Updated
    story: editableMemberStory.value.story || editableMemberStory.value.story, // Updated

    photoUrl: editableMemberStory.value.photoUrl, // Updated
    tags: editableMemberStory.value.tags, // Updated
    keywords: editableMemberStory.value.keywords, // Updated
  };

  const result = await memoryStore.updateItem(updatePayload); // Updated
  if (result.ok) {
    emit('saved', props.memberStoryId); // Updated
  } else {
    // Error is handled by the store, no specific action needed here
  }
  savingMemberStory.value = false; // Updated
};
onMounted(() => {
  if (props.memberStoryId) { // Updated
    loadMemberStory(props.memberStoryId); // Updated
  }
});

watch(
  () => props.memberStoryId, // Updated
  (newId) => {
    if (newId) {
      loadMemberStory(newId); // Updated
    } else {
      editableMemberStory.value = null; // Updated
    }
  },
);
</script>

<style scoped>
/* Scoped styles for MemberStoryEdit */
</style>
