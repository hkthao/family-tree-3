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
            <div v-if="selectedMemberDetails">
              <div class="text-subtitle-1">{{ selectedMemberDetails.fullName }}</div>
              <div class="text-caption text-medium-emphasis">{{ selectedMemberDetails.birthDeathYears }}</div>
            </div>
            </div>
          </div>
        </v-card>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="secondary" @click="$emit('update:show', false)">{{ t('common.cancel') }}</v-btn>
        <v-btn color="primary" :disabled="!props.disableSaveValidation && !selectedMemberId" @click="handleSave">{{ t('common.save') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { DetectedFace, Member } from '@/types';
import { useFaceMemberSelectDialog } from '@/composables';

const props = defineProps({
  show: { type: Boolean, required: true },
  selectedFace: { type: Object as () => DetectedFace | null, default: null },
  familyId: { type: String, default: undefined },
  showRelationPromptField: { type: Boolean, default: false },
  disableSaveValidation: { type: Boolean, default: false }, // NEW PROP
});

const emit = defineEmits<{
  (e: 'update:show', value: boolean): void;
  (e: 'label-face', updatedFace: DetectedFace): void; // Changed to pass updatedFace
}>();

const {
  t,
  selectedMemberId,
  selectedMemberDetails,
  internalRelationPrompt,
  faceThumbnailSrc,
  handleSave,
} = useFaceMemberSelectDialog(props, emit);
</script>
