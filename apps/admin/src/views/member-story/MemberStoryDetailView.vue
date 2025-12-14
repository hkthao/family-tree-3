<template>
  <div>
    <v-card v-if="memberStory" flat>
      <v-card-title class="text-h5 text-uppercase">{{ t('memberStory.detail.viewTitle') }}</v-card-title>
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
      <MemberStoryForm v-model="memberStory" :readonly="true" />

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

    <v-alert v-else-if="isError" type="error" dismissible class="mt-4">
      {{ error?.message || t('memberStory.detail.loadError') }}
    </v-alert>
    <v-progress-linear v-else-if="isLoading" indeterminate color="primary" class="mt-4"></v-progress-linear>
  </div>
</template>

<style scoped>
.text-shadow {
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.6);
}
</style>


<script setup lang="ts">
import { toRef, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryQuery } from '@/composables/memberStory';
import { useGlobalSnackbar } from '@/composables';
import type { MemberStoryDto } from '@/types/memberStory';
import { getAvatarUrl } from '@/utils/avatar.utils';
import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue'; // Import MemberStoryForm

const props = defineProps<{
  memberStoryId: string;
}>();

const emit = defineEmits(['close', 'edit-item']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { data: memberStory, isLoading, isError, error } = useMemberStoryQuery(toRef(props, 'memberStoryId'));

watch([isLoading, isError, memberStory], () => {
  if (!isLoading.value && isError.value) {
    showSnackbar(error.value?.message || t('memberStory.detail.notFound'), 'error');
  } else if (!isLoading.value && !memberStory.value) {
    showSnackbar(t('memberStory.detail.notFound'), 'error');
  }
}, { immediate: true });
</script>
<style scoped>
.meta-data {
  margin: 0px !important;
}
</style>