<template>
  <v-card-title class="text-center">
    <span class="text-h6">{{ memberStory?.title || t('memberStory.detail.titleDefault') }}</span>
  </v-card-title>
  <v-card-text>
    <MemberStoryForm
      v-if="memberStory"
      v-model="memberStory"
      :readonly="true"
      :member-id="memberStory.memberId"
    />
    <v-alert v-else-if="memberStoryStore.error" type="error" dismissible class="mb-4">
      {{ memberStoryStore.error }}
    </v-alert>
    <v-alert v-else type="info">{{ t('memberStory.detail.loading') }}</v-alert>
  </v-card-text>
  <v-card-actions>
    <v-spacer></v-spacer>
    <v-btn color="blue-darken-1" variant="text" @click="emit('close')">
      {{ t('common.close') }}
    </v-btn>
    <v-btn color="blue-darken-1" variant="text" @click="emit('edit-item', memberStory?.id)">
      {{ t('common.edit') }}
    </v-btn>
  </v-card-actions>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store'; // Updated
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemberStoryDto } from '@/types/memberStory'; // Make sure this import is correct // Updated
import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue'; // Updated

const props = defineProps<{
  memberStoryId: string; // Updated
}>();

const emit = defineEmits(['close', 'edit-item']);

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore(); // Updated
const { showSnackbar } = useGlobalSnackbar();

const memberStory = ref<MemberStoryDto | null>(null); // Updated

const fetchMemberStory = async (id: string) => { // Updated
  memberStory.value = await memberStoryStore.getById(id) || null; // Updated
  if (!memberStory.value) { // Updated
    showSnackbar(t('memberStory.detail.notFound'), 'error'); // Updated
  }
};

onMounted(() => {
  if (props.memberStoryId) { // Updated
    fetchMemberStory(props.memberStoryId); // Updated
  }
});

watch(() => props.memberStoryId, (newId) => { // Updated
  if (newId) {
    fetchMemberStory(newId); // Updated
  }
});
</script>
