<template>
  <v-container>
    <v-card class="pa-4">
      <v-card-title class="text-h5">Xác định Quan hệ Gia phả</v-card-title>
      <v-card-text>
        <v-form @submit.prevent="detectRelationship">
          <v-row>
            <v-col cols="12">
              <FamilyAutocomplete
                v-model="selectedFamilyId"
                label="Chọn Gia đình"
                :rules="[(v: string | undefined | null) => !!v || 'Gia đình là bắt buộc']"
                clearable
                required
                class="mb-4"
              />
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6">
              <MemberAutocomplete
                v-model="selectedMemberAId"
                label="Chọn Thành viên A"
                :family-id="selectedFamilyId"
                :rules="[(v: string | undefined | null) => !!v || 'Thành viên A là bắt buộc']"
                clearable
                required
                class="mb-4"
              />
            </v-col>
            <v-col cols="12" md="6">
              <MemberAutocomplete
                v-model="selectedMemberBId"
                label="Chọn Thành viên B"
                :family-id="selectedFamilyId"
                :rules="[(v: string | undefined | null) => !!v || 'Thành viên B là bắt buộc']"
                clearable
                required
                class="mb-4"
              />
            </v-col>
          </v-row>
          <v-row justify="end">
            <v-col cols="auto">
              <v-btn
                type="submit"
                color="primary"
                :loading="loading"
                :disabled="!selectedFamilyId || !selectedMemberAId || !selectedMemberBId"
                class="mt-4"
              >
                Xác định Quan hệ
              </v-btn>
            </v-col>
          </v-row>
        </v-form>

        <v-divider class="my-6"></v-divider>

        <div v-if="result">
          <v-card-title class="text-h6">Kết quả Quan hệ</v-card-title>
          <v-card-text>
            <p><strong>Từ A đến B:</strong> {{ result.fromAToB }}</p>
            <p><strong>Từ B đến A:</strong> {{ result.fromBToA }}</p>
            <p><strong>Đường đi (IDs):</strong> {{ result.path.join(' -> ') }}</p>
            <p><strong>Các cạnh (kiểu):</strong> {{ result.edges.join(' -> ') }}</p>
          </v-card-text>
        </div>

        <v-alert v-if="error" type="error" dense dismissible class="mt-4">
          {{ error }}
        </v-alert>

        <v-alert v-if="!result && !error && !loading" type="info" dense class="mt-4">
          Vui lòng chọn gia đình và hai thành viên để xác định quan hệ.
        </v-alert>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useRelationshipDetectionStore } from '@/stores/relationshipDetection.store'; // Will create this later

const relationshipDetectionStore = useRelationshipDetectionStore();

const selectedFamilyId = ref<string | undefined>(undefined);
const selectedMemberAId = ref<string | undefined>(undefined);
const selectedMemberBId = ref<string | undefined>(undefined);

const result = ref<any | null>(null); // Use a more specific type later
const loading = ref(false);
const error = ref<string | null>(null);

// Reset members when family changes
watch(selectedFamilyId, () => {
  selectedMemberAId.value = undefined;
  selectedMemberBId.value = undefined;
  result.value = null;
  error.value = null;
});

const detectRelationship = async () => {
  if (!selectedFamilyId.value || !selectedMemberAId.value || !selectedMemberBId.value) {
    error.value = 'Vui lòng chọn đầy đủ Gia đình và 2 Thành viên.';
    return;
  }

  loading.value = true;
  error.value = null;
  result.value = null;

  try {
    // Call the store action to detect relationship
    const detectionResult = await relationshipDetectionStore.detectRelationship(
      selectedFamilyId.value,
      selectedMemberAId.value,
      selectedMemberBId.value
    );
    if (detectionResult) {
        result.value = detectionResult;
        if (result.value.fromAToB === 'unknown' && result.value.fromBToA === 'unknown') {
            error.value = 'Không tìm thấy quan hệ giữa hai thành viên.';
            result.value = null; // Clear result if unknown
        }
    } else {
        error.value = 'Không tìm thấy quan hệ giữa hai thành viên.';
    }
  } catch (err: any) {
    error.value = err.message || 'Đã xảy ra lỗi khi xác định quan hệ.';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
/* Add any specific styles here if needed */
</style>
