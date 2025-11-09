<template>
  <v-dialog :model-value="show" @update:model-value="$emit('update:show', $event)" max-width="500px">
    <v-card>
      <v-card-title class="text-h6">{{ t('face.selectMemberDialog.title') }}</v-card-title>
      <v-card-text>
        <member-auto-complete v-model="selectedMemberId" :label="t('face.selectMemberDialog.selectMember')"
           :clearable="true" />
        <v-card v-if="selectedMemberDetails" class="mt-4 pa-3" variant="outlined">
          <div class="d-flex align-center">
            <v-avatar size="48" rounded="lg" class="mr-3">
              <v-img :src="faceThumbnailSrc" alt="Detected Face"></v-img>
            </v-avatar>
            <div>
              <div class="text-subtitle-1">{{ selectedMemberDetails.fullName }}</div>
              <div class="text-caption text-medium-emphasis">{{ selectedMemberDetails.birthDeathYears }}</div>
            </div>
          </div>
        </v-card>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="secondary" @click="$emit('update:show', false)">{{ t('common.cancel') }}</v-btn>
        <v-btn color="primary" :disabled="!selectedMemberId" @click="handleSave">{{ t('common.save') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, Member } from '@/types';
import { useMemberStore } from '@/stores/member.store';

const { t } = useI18n();
const memberStore = useMemberStore();

const props = defineProps({
  show: { type: Boolean, required: true },
  selectedFace: { type: Object as () => DetectedFace | null, default: null },
});

const emit = defineEmits<{ (e: 'update:show', value: boolean): void; (e: 'label-face', faceId: string, memberDetails: Member): void; }>();

const selectedMemberId = ref<string | null | undefined>(undefined);
const selectedMemberDetails = ref<Member | null>(null); // To store full member details

watch(() => props.selectedFace, (newFace) => {
  selectedMemberId.value = newFace?.memberId;
}, { immediate: true });

watch(selectedMemberId, async (newMemberId) => {
  if (newMemberId) {
    await memberStore.getById(newMemberId);
    selectedMemberDetails.value = memberStore.detail.item || null;
  } else {
    selectedMemberDetails.value = null;
  }
}, { immediate: true });

const faceThumbnailSrc = computed(() => {
  if (props.selectedFace?.thumbnail) {
    // Assuming the thumbnail is always a JPEG base64 string. Adjust if other formats are possible.
    return `data:image/jpeg;base64,${props.selectedFace.thumbnail}`;
  }
  return '';
});

const handleSave = () => {
  if (props.selectedFace && selectedMemberId.value && selectedMemberDetails.value) {
    emit('label-face', props.selectedFace.id, selectedMemberDetails.value);
    emit('update:show', false);
  }
};
</script>
