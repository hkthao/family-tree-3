<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card class="pa-4">
          <v-card-title class="text-h5">{{ $t('chunkAdmin.title') }}</v-card-title>
          <v-card-text>
            <v-row>
              <v-col cols="12">
                <ChunkUpload @file-selected="handleFileSelected" />
              </v-col>
              <v-col cols="12">
                <ChunkMetadataForm v-model:fileId="fileId" v-model:familyId="familyId" v-model:category="category"
                  v-model:createdBy="createdBy" ref="chunkMetadataFormRef" />
              </v-col>
              <v-col cols="12">
                <v-btn class="mr-2" color="primary" :disabled="!isFormValid || chunkStore.loading" @click="upload">
                  {{ $t('chunkUpload.uploadButton') }}
                </v-btn>
                <v-btn color="secondary" :disabled="chunkStore.loading" @click="resetForm">
                  {{ $t('chunkUpload.resetButton') }}
                </v-btn>
              </v-col>
            </v-row>
            <v-progress-linear v-if="chunkStore.loading" indeterminate color="primary" class="my-3"></v-progress-linear>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row v-if="chunkStore.chunks.length > 0">
      <v-col cols="12">
        <ChunkTable :chunks="chunkStore.chunks" @chunk-approval-changed="handleChunkApprovalChange"
          @approve-selected="handleApproveSelected" @reject-selected="handleRejectSelected" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useChunkStore } from '@/stores/chunk.store';
import { useAuthStore } from '@/stores/auth.store';
import { useNotificationStore } from '@/stores/notification.store';
import { ChunkUpload, ChunkTable, ChunkMetadataForm } from '@/components/chunks';

const chunkStore = useChunkStore();
const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const { t } = useI18n();

const selectedFile = ref<File | undefined>(undefined);
const fileId = ref('');
const familyId = ref('');
const category = ref('');
const createdBy = ref('');

const chunkMetadataFormRef = ref<InstanceType<typeof ChunkMetadataForm> | null>(null);

const handleFileSelected = (file: File) => {
  selectedFile.value = file;
  fileId.value = crypto.randomUUID(); // Generate a new UUID when file is selected
};

const isFormValid = computed(() => {
  return (
    !!selectedFile.value &&
    chunkMetadataFormRef.value?.validate()
  );
});

const upload = async () => {
  if (selectedFile.value && isFormValid.value) {
    const metadata = {
      fileId: fileId.value,
      familyId: familyId.value,
      category: category.value,
      createdBy: createdBy.value,
    };
    await chunkStore.uploadFile(selectedFile.value, metadata);
    if (!chunkStore.error) {
      notificationStore.showSnackbar(t('chunkAdmin.uploadSuccess', { count: chunkStore.chunks.length }), 'success');
    }
  }
};

const resetForm = () => {
  selectedFile.value = undefined;
  fileId.value = '';
  familyId.value = '';
  category.value = '';
};

const handleChunkApprovalChange = (chunkId: string, approved: boolean) => {
  chunkStore.setChunkApproval(chunkId, approved);
  notificationStore.showSnackbar(t('chunkAdmin.chunkApprovalChange', { chunkId, status: approved ? t('common.approved') : t('common.rejected') }), 'info');
};

const handleApproveSelected = async (chunkIds: string[]) => {
  const chunksToApprove = chunkStore.chunks.filter(chunk => chunkIds.includes(chunk.id));

  if (chunksToApprove.length > 0) {
    await chunkStore.approveChunks(chunksToApprove);
    if (!chunkStore.error) {
      // Update local approval status only after successful API call
      chunkIds.forEach(id => chunkStore.setChunkApproval(id, true));
      notificationStore.showSnackbar(t('chunkAdmin.approveSelectedSuccess', { count: chunksToApprove.length }), 'success');
    } else {
      // If there's an error from approveChunks, show it
      notificationStore.showSnackbar(chunkStore.error, 'error');
    }
  } else {
    notificationStore.showSnackbar(t('chunkAdmin.noChunksToApprove'), 'warning'); // Add a new i18n key
  }
};

const handleRejectSelected = (chunkIds: string[]) => {
  chunkIds.forEach(id => chunkStore.setChunkApproval(id, false));
  notificationStore.showSnackbar(t('chunkAdmin.rejectSelectedSuccess', { count: chunkIds.length }), 'warning');
};

// Clear form after successful upload
watch(() => chunkStore.chunks, (newChunks) => {
  if (newChunks.length > 0 && !chunkStore.error) {
    selectedFile.value = undefined;
    fileId.value = '';
    familyId.value = '';
    category.value = '';
    // createdBy.value = ''; // Keep createdBy as it's auto-filled
  }
});

// Watch for chunkStore.error to show global notification
watch(() => chunkStore.error, (newError) => {
  if (newError) {
    notificationStore.showSnackbar(newError, 'error');
  }
});

// Auto-fill createdBy on mount and when authStore.user changes
onMounted(() => {
  if (authStore.user?.id) {
    createdBy.value = authStore.user.id;
  }
});

watch(() => authStore.user, (newUser) => {
  if (newUser?.id) {
    createdBy.value = newUser.id;
  }
});

</script>