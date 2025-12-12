<template>
  <v-card class="face-label-card" elevation="2">
    <v-card-title class="text-h6">{{ t('face.labelCard.title') }}</v-card-title>
    <v-card-text>
      <div class="d-flex justify-center mb-4">
        <v-avatar size="96" rounded="lg">
          <v-img :src="face.thumbnail" alt="Cropped Face"></v-img>
        </v-avatar>
      </div>

      <v-autocomplete
        v-model="selectedMemberId"
        :items="members"
        item-title="fullName"
        item-value="id"
        :label="t('face.labelCard.selectMember')"
        :placeholder="t('face.labelCard.searchMemberPlaceholder')"
        prepend-inner-icon="mdi-magnify"
        variant="outlined"
        clearable
        hide-details
        class="mb-4"
      >
        <template v-slot:item="{ props, item }">
          <v-list-item
            v-bind="props"
            :prepend-avatar="item.raw.avatarUrl"
            :title="item.raw.fullName"
            :subtitle="item.raw.id"
          ></v-list-item>
        </template>
      </v-autocomplete>

      <v-btn
        block
        color="primary"
        :disabled="!selectedMemberId"
        @click="handleSaveMapping"
        class="mb-2"
      >
        {{ t('face.labelCard.saveButton') }}
      </v-btn>

      <v-divider class="my-4"></v-divider>

      <v-btn block @click="showCreateMemberDialog = true">
        {{ t('face.labelCard.createNewMemberButton') }}
      </v-btn>
    </v-card-text>

    <v-dialog v-model="showCreateMemberDialog" max-width="500px">
      <v-card>
        <v-card-title>{{ t('face.labelCard.createMemberTitle') }}</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newMemberName"
            :label="t('member.form.fullName')"
            variant="outlined"
            hide-details
            class="mb-4"
          ></v-text-field>
          <!-- Add more fields for new member if necessary -->
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="secondary" @click="showCreateMemberDialog = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" :disabled="!newMemberName" @click="handleCreateNewMember">{{ t('common.create') }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, Member } from '@/types';

const { t } = useI18n();

const props = defineProps({
  face: { type: Object as () => DetectedFace, required: true },
  members: { type: Array as () => Member[], default: () => [] }, // New prop for available members
});

const emit = defineEmits(['label-face', 'create-member']); // Updated emits

const selectedMemberId = ref<string | null | undefined>(props.face.memberId);
const showCreateMemberDialog = ref(false);
const newMemberName = ref('');

watch(() => props.face.memberId, (newMemberId) => {
  selectedMemberId.value = newMemberId;
});

const handleSaveMapping = () => {
  if (selectedMemberId.value) {
    emit('label-face', props.face.id, selectedMemberId.value);
  }
};

const handleCreateNewMember = () => {
  if (newMemberName.value) {
    emit('create-member', props.face.id, newMemberName.value);
    showCreateMemberDialog.value = false;
    newMemberName.value = '';
    // The parent component will update the detectedFaces array,
    // which will then update selectedMemberId via the watch on props.face.memberId
  }
};
</script>

<style scoped>
.face-label-card {
  height: 100%;
}
</style>
