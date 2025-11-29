<template>
  <v-container fluid>
    <v-card v-if="memberStory" flat>
      <!-- Cover Image -->
      <v-img v-if="memberStory.resizedImageUrl" :src="memberStory.resizedImageUrl" cover class="mb-4">
        <v-row class="fill-height align-end meta-data">
          <v-col class="pa-4" style="background: rgba(0, 0, 0, 0.4);">
            <h1 class="text-h4 text-white text-shadow">{{ memberStory.title || t('memberStory.detail.titleDefault') }}
            </h1>
            <div class="d-flex align-center text-white mt-2">
              <v-avatar size="28" class="mr-2">
                <v-img :src="memberStory.memberAvatarUrl || 'https://via.placeholder.com/150'"
                  alt="Member Avatar"></v-img>
              </v-avatar>
              <span class="text-body-2 font-weight-bold">{{ memberName }}</span>
              <v-icon size="small" class="mx-2">mdi-circle-small</v-icon>
              <span class="text-caption" v-if="memberStory.createdAt">{{ new
                Date(memberStory.createdAt).toLocaleDateString() }}</span>
            </div>
          </v-col>
        </v-row>
      </v-img>

      <v-card-text class="pa-0">
        <!-- Story Content -->
        <div class="mb-6">
          <p class="text-body-1 text-justify">{{ memberStory.story }}</p>
        </div>

        <!-- Short Description (Raw Input) -->
        <div class="mb-6" v-if="memberStory.rawInput">
          <h2 class="text-h5 mb-2">{{ t('memberStory.detail.shortDescription') }}</h2>
          <p class="text-body-2 text-medium-emphasis text-justify">{{ memberStory.rawInput }}</p>
        </div>

        <!-- Style and Perspective Chips -->
        <div class="mb-6">
          <v-chip-group>
            <v-chip v-if="memberStory.storyStyle" color="primary" variant="outlined">{{ storyStyleText }}</v-chip>
            <v-chip v-if="memberStory.perspective" color="secondary" variant="outlined">{{ perspectiveText }}</v-chip>
          </v-chip-group>
        </div>
      </v-card-text>

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
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemberStoryDto } from '@/types/memberStory';
import { useMemberStore } from '@/stores/member.store'; // NEW
import type { Member } from '@/types'; // NEW

const props = defineProps<{
  memberStoryId: string;
}>();

const emit = defineEmits(['close', 'edit-item']);

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore();
const memberStore = useMemberStore(); // NEW
const { showSnackbar } = useGlobalSnackbar();

const memberStory = ref<MemberStoryDto | null>(null);
const memberName = ref<string | null>(null); // NEW

const fetchMemberStory = async (id: string) => {
  memberStory.value = await memberStoryStore.getById(id) || null;
  if (memberStory.value?.memberId) {
    const member: Member | undefined = await memberStore.getById(memberStory.value.memberId);
    memberName.value = member?.fullName || null;
  } else {
    memberName.value = null;
  }

  if (!memberStory.value) {
    showSnackbar(t('memberStory.detail.notFound'), 'error');
  }
};

const storyStyleText = computed(() => { // NEW
  if (!memberStory.value?.storyStyle) return '';
  // Assuming a similar structure for story styles as in useMemberStoryForm
  // TODO: Centralize these definitions or retrieve from a shared source
  const styles = {
    nostalgic: t('memberStory.style.nostalgic'),
    warm: t('memberStory.style.warm'),
    formal: t('memberStory.style.formal'),
    folk: t('memberStory.style.folk'),
  };
  return styles[memberStory.value.storyStyle as keyof typeof styles] || memberStory.value.storyStyle;
});

const perspectiveText = computed(() => { // NEW
  if (!memberStory.value?.perspective) return '';
  // Assuming a similar structure for perspectives as in useMemberStoryForm
  // TODO: Centralize these definitions or retrieve from a shared source
  const perspectives = {
    firstPerson: t('memberStory.create.perspective.firstPerson'),
    neutralPersonal: t('memberStory.create.perspective.neutralPersonal'),
    fullyNeutral: t('memberStory.create.perspective.fullyNeutral'),
  };
  return perspectives[memberStory.value.perspective as keyof typeof perspectives] || memberStory.value.perspective;
});

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