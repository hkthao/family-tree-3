<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 mb-4">Quản lý Chunk tài liệu</h1>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <ChunkUpload />
      </v-col>
      <v-col cols="12" md="6">
        <v-card class="pa-4">
          <v-card-title class="text-h5">Trạng thái Chunk</v-card-title>
          <v-card-text>
            <v-alert v-if="chunkStore.loading" type="info" dense class="mb-3">Đang xử lý tệp...</v-alert>
            <v-alert v-if="chunkStore.error" type="error" dense dismissible class="mb-3">{{ chunkStore.error }}</v-alert>
            <v-alert v-if="chunkStore.chunks.length > 0 && !chunkStore.loading && !chunkStore.error" type="success" dense class="mb-3">
              Đã tải lên và xử lý {{ chunkStore.chunks.length }} chunk thành công.
            </v-alert>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row v-if="chunkStore.chunks.length > 0">
      <v-col cols="12">
        <ChunkTable
          :chunks="chunkStore.chunks"
          @chunk-approval-changed="handleChunkApprovalChange"
          @approve-selected="handleApproveSelected"
          @reject-selected="handleRejectSelected"
        />
      </v-col>
    </v-row>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
      {{ snackbar.message }}
      <template v-slot:actions>
        <v-btn variant="text" @click="snackbar.show = false">Đóng</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { reactive } from 'vue';
import { useChunkStore, TextChunk } from '@/stores/chunk.store';
import ChunkUpload from '@/components/ChunkUpload.vue';
import ChunkTable from '@/components/ChunkTable.vue';

const chunkStore = useChunkStore();

const snackbar = reactive({
  show: false,
  message: '',
  color: '',
});

const handleChunkApprovalChange = (chunkId: string, approved: boolean) => {
  chunkStore.setChunkApproval(chunkId, approved);
  snackbar.message = `Chunk ${chunkId} đã được ${approved ? 'phê duyệt' : 'từ chối'}.`;
  snackbar.color = 'info';
  snackbar.show = true;
};

const handleApproveSelected = (chunkIds: string[]) => {
  chunkIds.forEach(id => chunkStore.setChunkApproval(id, true));
  snackbar.message = `Đã phê duyệt ${chunkIds.length} chunk đã chọn.`;
  snackbar.color = 'success';
  snackbar.show = true;
};

const handleRejectSelected = (chunkIds: string[]) => {
  chunkIds.forEach(id => chunkStore.setChunkApproval(id, false));
  snackbar.message = `Đã từ chối ${chunkIds.length} chunk đã chọn.`;
  snackbar.color = 'warning';
  snackbar.show = true;
};

// Optional: Add a method to save approved chunks to Pinecone (future step)
// const saveApprovedChunks = async () => {
//   const approved = chunkStore.approvedChunks;
//   // Call another API or store action to save these chunks
//   console.log('Saving approved chunks:', approved);
//   snackbar.message = 'Đang lưu các chunk đã phê duyệt...';
//   snackbar.color = 'info';
//   snackbar.show = true;
//   // After saving, clear chunks
//   chunkStore.clearChunks();
// };
</script>
