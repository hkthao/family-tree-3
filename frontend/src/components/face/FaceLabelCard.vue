<template>
  <v-card class="face-label-card" elevation="2">
    <v-card-title class="text-h6">{{ t('face.labelCard.title') }}</v-card-title>
    <v-card-text>
      <div class="d-flex justify-center mb-4">
        <v-avatar size="96" rounded="lg">
          <v-img :src="face.imageUrl" alt="Cropped Face"></v-img>
        </v-avatar>
      </div>

      <v-alert v-if="faceStore.error" type="error" class="mb-4">{{ faceStore.error }}</v-alert>

      <v-autocomplete
        v-model="selectedMemberId"
        :items="availableMembers"
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
        :disabled="!selectedMemberId || faceStore.loading"
        :loading="faceStore.loading"
        @click="handleSaveMapping"
        class="mb-2"
      >
        {{ t('face.labelCard.saveButton') }}
      </v-btn>

      <v-divider class="my-4"></v-divider>

      <v-btn block variant="outlined" @click="showCreateMemberDialog = true">
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
import { useFaceStore } from '@/stores/face.store';
import type { DetectedFace, Member } from '@/types/face.d.ts';

const { t } = useI18n();
const faceStore = useFaceStore();

const props = defineProps({
  face: { type: Object as () => DetectedFace, required: true },
});

const emit = defineEmits(['save-mapping', 'create-new-member']);

const selectedMemberId = ref<string | null>(props.face.memberId);
const showCreateMemberDialog = ref(false);
const newMemberName = ref('');

// Mock data for available members (replace with actual data from a member store or API)
const availableMembers = computed<Member[]>(() => [
  { id: 'member123', fullName: 'John Doe', avatarUrl: 'https://randomuser.me/api/portraits/men/1.jpg' },
  { id: 'member456', fullName: 'Jane Smith', avatarUrl: 'https://randomuser.me/api/portraits/women/2.jpg' },
  { id: 'member789', fullName: 'Peter Jones', avatarUrl: 'https://randomuser.me/api/portraits/men/3.jpg' },
]);

watch(() => props.face.memberId, (newMemberId) => {
  selectedMemberId.value = newMemberId;
});

const handleSaveMapping = () => {
  if (selectedMemberId.value) {
    emit('save-mapping', props.face.id, selectedMemberId.value);
  }
};

const handleCreateNewMember = () => {
  if (newMemberName.value) {
    // In a real application, this would involve an API call to create a new member
    // and then mapping the face to this new member.
    // For now, we'll just emit the event.
    console.log('Creating new member:', newMemberName.value);
    // Simulate a new member ID for demonstration
    const newId = `new-member-${Date.now()}`;
    emit('create-new-member', props.face.id, { id: newId, fullName: newMemberName.value });
    showCreateMemberDialog.value = false;
    newMemberName.value = '';
    selectedMemberId.value = newId; // Automatically select the newly created member
  }
};
</script>

<style scoped>
.face-label-card {
  height: 100%;
}
</style>
