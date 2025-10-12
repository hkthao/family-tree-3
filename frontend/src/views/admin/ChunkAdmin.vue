<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card>
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
              <v-col cols="12" md="3">
                <v-btn color="primary" :disabled="!isFormValid || chunkStore.loading" @click="upload">
                  {{ $t('chunkUpload.uploadButton') }}
                </v-btn>
              </v-col>
            </v-row>
            <v-progress-linear v-if="chunkStore.loading" indeterminate color="primary" class="mb-3"></v-progress-linear>
            <v-alert v-if="chunkStore.error" type="error" dense dismissible class="mb-3">{{ chunkStore.error
              }}</v-alert>
            <v-alert v-if="chunkStore.chunks.length > 0 && !chunkStore.loading && !chunkStore.error" type="success"
              dense class="mb-3">
              {{ $t('chunkAdmin.uploadSuccess', { count: chunkStore.chunks.length }) }}
            </v-alert>
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

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
      {{ snackbar.message }}
      <template v-slot:actions>
        <v-btn variant="text" @click="snackbar.show = false">{{ $t('common.close') }}</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { reactive, ref, computed, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useChunkStore } from '@/stores/chunk.store';
import { useAuthStore } from '@/stores/auth.store';
import ChunkUpload from '@/components/ChunkUpload.vue';
import ChunkTable from '@/components/ChunkTable.vue';

const chunkStore = useChunkStore();
const authStore = useAuthStore();
const { t } = useI18n();

const selectedFile = ref<File | null>(null);
const fileId = ref('');
const familyId = ref('');
const category = ref('');
const createdBy = ref('');

const snackbar = reactive({
  show: false,
  message: '',
  color: '',
});

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
  }
};

const handleChunkApprovalChange = (chunkId: string, approved: boolean) => {
  chunkStore.setChunkApproval(chunkId, approved);
  snackbar.message = t('chunkAdmin.chunkApprovalChange', { chunkId, status: approved ? t('common.approved') : t('common.rejected') });
  snackbar.color = 'info';
  snackbar.show = true;
};

const handleApproveSelected = (chunkIds: string[]) => {
  chunkIds.forEach(id => chunkStore.setChunkApproval(id, true));
  snackbar.message = t('chunkAdmin.approveSelectedSuccess', { count: chunkIds.length });
  snackbar.color = 'success';
  snackbar.show = true;
};

const handleRejectSelected = (chunkIds: string[]) => {
  chunkIds.forEach(id => chunkStore.setChunkApproval(id, false));
  snackbar.message = t('chunkAdmin.rejectSelectedSuccess', { count: chunkIds.length });
  snackbar.color = 'warning';
  snackbar.show = true;
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
</script>
