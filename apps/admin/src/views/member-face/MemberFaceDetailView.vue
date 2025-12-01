<template>
  <div>

    <v-card :elevation="0" v-if="memberFace">
      <v-card-title class="text-center text-h5 text-uppercase mb-2">
        {{ t('memberFace.detail.title') }}
      </v-card-title>
      <v-card-text>
        <div v-if="memberFace" class="d-flex flex-column align-center text-center">
          <!-- Face Thumbnail -->
          <v-avatar size="120" rounded="lg" class="mb-4">
            <v-img :src="memberFace.thumbnailUrl || '/family_avatar.png'"
              :alt="memberFace.memberName || 'Face'"></v-img>
          </v-avatar>

          <!-- Member Name -->
          <v-card-title class="text-h5 text-uppercase mb-1">
            {{ memberFace.memberName || t('common.unknown') }}
          </v-card-title>

          <!-- Family Name (if available) -->
          <FamilyName v-if="memberFace.familyName" :name="memberFace.familyName" :avatar-url="memberFace.familyAvatarUrl" />

          <v-chip v-if="memberFace.emotion" color="info" size="small" class="mb-4">
            {{ memberFace.emotion }}
          </v-chip>

          <v-divider class="my-4 w-100"></v-divider>

          <!-- Other Details in a List -->
          <v-list class="w-100">
            <v-list-item v-if="memberFace.originalImageUrl">
              <v-list-item-title class="font-weight-medium">{{ t('memberFace.form.originalImage')}}:</v-list-item-title>
              <v-list-item-subtitle>
                <a :href="memberFace.originalImageUrl" target="_blank">{{ t('memberFace.form.viewOriginal') }}</a>
              </v-list-item-subtitle>
            </v-list-item>
            <!-- Add more fields as needed -->
          </v-list>
        </div>
        <v-alert v-else type="info" class="mt-4">{{ t('memberFace.detail.notFound') }}</v-alert>
      </v-card-text>

      <v-card-actions class="justify-end">
              <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>        <v-btn v-if="canPerformActions" color="error" @click="handleDelete" :disabled="detail.loading">{{ t('common.delete') }}</v-btn>
      </v-card-actions>
    </v-card>
    <v-progress-linear v-else-if="detail.loading" indeterminate color="primary"></v-progress-linear>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberFaceStore } from '@/stores/member-face.store';
import { MemberFaceForm } from '@/components/member-face'; // Will be created later
import type { MemberFace } from '@/types';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import FamilyName from '@/components/common/FamilyName.vue';
import { useAuth } from '@/composables/useAuth'; // NEW

interface MemberFaceDetailViewProps {
  memberFaceId: string;
}

const props = defineProps<MemberFaceDetailViewProps>();
const emit = defineEmits(['close', 'edit-member-face', 'member-face-deleted']);

const { t } = useI18n();
const memberFaceStore = useMemberFaceStore();
const { showSnackbar } = useGlobalSnackbar();
const { showConfirmDialog } = useConfirmDialog();
const { isAdmin, isFamilyManager } = useAuth(); // NEW

const { detail } = storeToRefs(memberFaceStore);

const memberFace = ref<MemberFace | undefined>(undefined);

const canPerformActions = computed(() => { // NEW
  return isAdmin.value || isFamilyManager.value;
});

const loadMemberFace = async (id: string) => {
  await memberFaceStore.getById(id);
  if (memberFaceStore.detail.item) {
    memberFace.value = memberFaceStore.detail.item;
  } else {
    memberFace.value = undefined;
  }
};

onMounted(async () => {
  if (props.memberFaceId) {
    await loadMemberFace(props.memberFaceId);
  }
});

watch(
  () => props.memberFaceId,
  async (newId) => {
    if (newId) {
      await loadMemberFace(newId);
    }
  },
);

const handleClose = () => {
  emit('close');
};



const handleDelete = async () => {
  if (!memberFace.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('memberFace.list.confirmDelete', { faceId: memberFace.value.faceId }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    try {
      await memberFaceStore.deleteItem(memberFace.value.id);
      if (!memberFaceStore.delete.error) {
        showSnackbar(t('memberFace.messages.deleteSuccess'), 'success');
        emit('member-face-deleted'); // Notify parent that member face was deleted
        emit('close'); // Close the detail drawer
      } else {
        showSnackbar(memberFaceStore.delete.error.message || t('memberFace.messages.deleteError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('memberFace.messages.deleteError'), 'error');
    }
  }
};
</script>
