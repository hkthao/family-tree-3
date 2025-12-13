<template>
  <v-container fluid>
    <v-card v-if="memberStory" flat>
      <!-- Cover Image - Keep cover image display if preferred, or let form handle it -->
      <v-img v-if="memberStory.memberStoryImages && memberStory.memberStoryImages.length > 0 && memberStory.memberStoryImages[0].imageUrl" :src="memberStory.memberStoryImages[0].imageUrl" cover class="mb-4">
        <v-row class="fill-height align-end meta-data">
          <v-col class="pa-2" style="background: rgba(0, 0, 0, 0.4);">
            <h1 class="text-h4 text-white text-shadow">{{ memberStory.title || t('memberStory.detail.titleDefault') }}
            </h1>
            <div class="d-flex align-center text-white mt-2">
              <v-avatar size="28" class="mr-2">
                <v-img :src="getAvatarUrl(memberStory.memberAvatarUrl, memberStory.memberGender)"
                  alt="Member Avatar"></v-img>
              </v-avatar>
              <span class="text-body-2 font-weight-bold">{{ memberStory.memberFullName }}</span>
              <v-icon size="small" class="mx-2">mdi-circle-small</v-icon>
              <span class="text-caption" v-if="memberStory.createdAt">{{ new
                Date(memberStory.createdAt).toLocaleDateString() }}</span>
            </div>
          </v-col>
        </v-row> </v-img>

      <!-- MemberStoryForm in readonly mode -->
      <MemberStoryForm v-model="memberStory" :readonly="true" :family-id="memberStory?.familyId" />

      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn variant="text" @click="emit('close')">
          {{ t('common.close') }}
        </v-btn>
        <v-btn variant="text" @click="emit('edit-item', memberStory?.id)">
          {{ t('common.edit') }}
        </v-btn>
      </v-card-actions>
    </v-card>

    <v-alert v-else-if="memberStoryStore.error" type="error" dismissible class="mt-4">
      {{ memberStoryStore.error }}
    </v-alert>
    <v-progress-linear v-else indeterminate color="primary" class="mt-4"></v-progress-linear>
  </v-container>
</template>

<style scoped>
.text-shadow {
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.6);
}
</style>


<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store';
import { useGlobalSnackbar } from '@/composables';
import type { MemberStoryDto } from '@/types/memberStory';
import { getAvatarUrl } from '@/utils/avatar.utils';
import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue'; // Import MemberStoryForm

const props = defineProps<{
  memberStoryId: string;
}>();

const emit = defineEmits(['close', 'edit-item']);

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore();
const { showSnackbar } = useGlobalSnackbar();

const memberStory = ref<MemberStoryDto | null>(null);

const fetchMemberStory = async (id: string) => {
  memberStory.value = await memberStoryStore.getById(id) || null;
  if (!memberStory.value) {
    showSnackbar(t('memberStory.detail.notFound'), 'error');
  }
};

onMounted(() => {
  if (props.memberStoryId) {
    fetchMemberStory(props.memberStoryId);
  }
});

watch(() => props.memberStoryId, (newId) => {
  if (newId) {
    fetchMemberStory(newId);
  }
});
</script>
<style scoped>
.meta-data {
  margin: 0px !important;
}
</style>