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
              <v-col cols="12" md="6">
                <v-text-field v-model="fileId" :label="$t('chunkUpload.fileIdLabel')"
                  :rules="[v => !!v || $t('chunkUpload.fileIdRequired')]" required></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="familyId" :label="$t('chunkUpload.familyIdLabel')"></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="category" :label="$t('chunkUpload.categoryLabel')"
                  :rules="[v => !!v || $t('chunkUpload.categoryRequired')]" required></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="createdBy" :label="$t('chunkUpload.createdByLabel')"
                  :rules="[v => !!v || $t('chunkUpload.createdByRequired')]" required></v-text-field>
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
            <v-progress-linear v-if="chunkStore.loading" indeterminate color="primary" class="mb-3"></v-progress-linear>
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
import ChunkUpload from '@/components/ChunkUpload.vue';
import ChunkTable from '@/components/ChunkTable.vue';

const chunkStore = useChunkStore();
const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const { t } = useI18n();

const selectedFile = ref<File | null>(null);
const fileId = ref('');
const familyId = ref('');
const category = ref('');
const createdBy = ref('');

const handleFileSelected = (file: File) => {
  selectedFile.value = file;
  fileId.value = crypto.randomUUID(); // Generate a new UUID when file is selected
};

const isFormValid = computed(() => {
  return (
    !!selectedFile.value &&
    !!fileId.value &&
    !!category.value &&
    !!createdBy.value
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
  selectedFile.value = null;
  fileId.value = '';
  familyId.value = '';
  category.value = '';
  // createdBy.value = ''; // Keep createdBy as it's auto-filled
};

const handleChunkApprovalChange = (chunkId: string, approved: boolean) => {
  chunkStore.setChunkApproval(chunkId, approved);
  notificationStore.showSnackbar(t('chunkAdmin.chunkApprovalChange', { chunkId, status: approved ? t('common.approved') : t('common.rejected') }), 'info');
};

const handleApproveSelected = (chunkIds: string[]) => {
  chunkIds.forEach(id => chunkStore.setChunkApproval(id, true));
  notificationStore.showSnackbar(t('chunkAdmin.approveSelectedSuccess', { count: chunkIds.length }), 'success');
};

const handleRejectSelected = (chunkIds: string[]) => {
  chunkIds.forEach(id => chunkStore.setChunkApproval(id, false));
  notificationStore.showSnackbar(t('chunkAdmin.rejectSelectedSuccess', { count: chunkIds.length }), 'warning');
};

// Clear form after successful upload
watch(() => chunkStore.chunks, (newChunks) => {
  if (newChunks.length > 0 && !chunkStore.error) {
    selectedFile.value = null;
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