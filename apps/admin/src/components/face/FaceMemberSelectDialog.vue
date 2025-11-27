<template>
  <v-dialog :model-value="show" @update:model-value="$emit('update:show', $event)" max-width="500px">
    <v-card>
      <v-card-title class="text-h6">{{ t('face.selectMemberDialog.title') }}</v-card-title>
      <v-card-text>
        <member-auto-complete v-model="selectedMemberId" :label="t('face.selectMemberDialog.selectMember')"
           :clearable="true" :family-id="props.familyId" />
        <v-text-field
          v-if="props.showRelationPromptField"
          v-model="internalRelationPrompt"
          :label="t('face.selectMemberDialog.relationPromptLabel')"
          class="mt-4"
        ></v-text-field>
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
import { useFaceMemberSelectStore } from '@/stores/faceMemberSelect.store';

const { t } = useI18n();
const faceMemberSelectStore = useFaceMemberSelectStore();

const props = defineProps({
  show: { type: Boolean, required: true },
  selectedFace: { type: Object as () => DetectedFace | null, default: null },
  familyId: { type: String, default: undefined },
  showRelationPromptField: { type: Boolean, default: false }, // New prop
});

const emit = defineEmits<{
  (e: 'update:show', value: boolean): void;
  (e: 'label-face', updatedFace: DetectedFace): void; // Changed to pass updatedFace
}>();

const selectedMemberId = ref<string | null | undefined>(undefined);
const selectedMemberDetails = ref<Member | null>(null);
const internalRelationPrompt = ref<string | undefined>(undefined); // New ref

watch(() => props.selectedFace, (newFace) => {
  selectedMemberId.value = newFace?.memberId;
  internalRelationPrompt.value = newFace?.relationPrompt; // Initialize from selectedFace
}, { immediate: true });

watch(selectedMemberId, async (newMemberId) => {
  if (newMemberId) {
    await faceMemberSelectStore.getById(newMemberId);
    selectedMemberDetails.value = faceMemberSelectStore.detail.item || null;
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
    const updatedFace: DetectedFace = {
      ...props.selectedFace,
      memberId: selectedMemberDetails.value.id,
      memberName: selectedMemberDetails.value.fullName,
      relationPrompt: internalRelationPrompt.value,
    };
    emit('label-face', updatedFace); // Emit the fully updated face object
    emit('update:show', false);
  }
};
</script>
