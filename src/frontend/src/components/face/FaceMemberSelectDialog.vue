<template>
  <v-dialog :model-value="show" @update:model-value="$emit('update:show', $event)" max-width="500px">
    <v-card>
      <v-card-title class="text-h6">{{ t('face.selectMemberDialog.title') }}</v-card-title>
      <v-card-text>
        <div v-if="selectedFace" class="d-flex flex-column align-center mb-4">
          <v-avatar size="96" rounded="lg" class="mb-2">
            <v-img :src="selectedFace.imageUrl" alt="Cropped Face"></v-img>
          </v-avatar>
          <p class="text-subtitle-1">{{ t('face.selectMemberDialog.labelFor') }} <strong>{{ selectedFace.id }}</strong></p>
        </div>

        <MemberAutocomplete
          v-model="selectedMemberId"
          :label="t('face.selectMemberDialog.selectMember')"
          :clearable="true"
          class="mt-4"
        />

        <v-card v-if="selectedMemberDetails" class="mt-4 pa-3" variant="outlined">
          <div class="d-flex align-center">
            <v-avatar size="48" rounded="lg" class="mr-3">
              <v-img :src="selectedMemberDetails.avatarUrl" alt="Member Avatar"></v-img>
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
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, Member } from '@/types';
import { MemberAutocomplete } from '@/components/common';

const { t } = useI18n();

const props = defineProps({
  show: { type: Boolean, required: true },
  selectedFace: { type: Object as () => DetectedFace | null, default: null },
  managedMembers: { type: Array as () => Member[], required: true }, // Added managedMembers prop
});

const emit = defineEmits(['update:show', 'label-face']);

const selectedMemberId = ref<string | null | undefined>(undefined);
const selectedMemberDetails = ref<Member | null>(null); // To store full member details

watch(() => props.selectedFace, (newFace) => {
  selectedMemberId.value = newFace?.memberId;
}, { immediate: true });

watch(selectedMemberId, (newMemberId) => {
  if (newMemberId) {
    selectedMemberDetails.value = props.managedMembers.find(m => m.id === newMemberId) || null;
  } else {
    selectedMemberDetails.value = null;
  }
}, { immediate: true });

const handleSave = () => {
  if (props.selectedFace && selectedMemberId.value) {
    emit('label-face', props.selectedFace.id, selectedMemberId.value);
    emit('update:show', false);
  }
};
</script>
