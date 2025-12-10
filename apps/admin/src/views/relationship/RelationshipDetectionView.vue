<template>
  <v-container>
    <v-card>
      <v-card-title class="text-h5">{{ t('relationshipDetection.title') }}</v-card-title>
      <v-card-text>
        <v-form @submit.prevent="detectRelationship">
          <v-row>
            <v-col cols="12">
              <FamilyAutocomplete v-model="selectedFamilyId" :label="t('relationshipDetection.familyLabel')"
                :rules="[(v: string | undefined | null) => !!v || t('relationshipDetection.familyRequired')]" clearable
                required />
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6">
              <MemberAutocomplete v-model="selectedMemberAId" :label="t('relationshipDetection.memberALabel')"
                :family-id="selectedFamilyId"
                :rules="[(v: string | undefined | null) => !!v || t('relationshipDetection.memberARequired')]" clearable
                required />
            </v-col>
            <v-col cols="12" md="6">
              <MemberAutocomplete v-model="selectedMemberBId" :label="t('relationshipDetection.memberBLabel')"
                :family-id="selectedFamilyId"
                :rules="[(v: string | undefined | null) => !!v || t('relationshipDetection.memberBRequired')]" clearable
                required />
            </v-col>
          </v-row>
          <v-row justify="end">
            <v-col cols="auto">
              <v-btn type="submit" color="primary" :loading="loading"
                :disabled="!selectedFamilyId || !selectedMemberAId || !selectedMemberBId">
                {{ t('relationshipDetection.detectButton') }}
              </v-btn>
            </v-col>
          </v-row>
        </v-form>

        <v-divider class="my-6"></v-divider>

        <div v-if="result">
          <v-card-title class="text-h6">{{ t('relationshipDetection.resultTitle') }}</v-card-title>
          <v-card-text>
            <p><strong>{{ t('relationshipDetection.descriptionLabel') }}:</strong> {{ result.description }}</p>
            <p><strong>{{ t('relationshipDetection.path') }}:</strong> {{ result.path.join(' -> ') }}</p>
            <p><strong>{{ t('relationshipDetection.edges') }}:</strong> {{ result.edges.join(' -> ') }}</p>
          </v-card-text>
        </div>

        <v-alert v-if="error" type="error" dense dismissible>
          {{ error }}
        </v-alert>

        <v-alert v-if="!result && !error && !loading" type="info" dense>
          {{ t('relationshipDetection.selectMembersPrompt') }}
        </v-alert>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useRelationshipDetectionStore } from '@/stores/relationshipDetection.store';

// Define the expected structure of RelationshipDetectionResult from the backend
interface RelationshipDetectionResult {
  description: string;
  path: string[]; // Assuming GUIDs are converted to strings for display
  edges: string[];
}

const { t } = useI18n();
const relationshipDetectionStore = useRelationshipDetectionStore();
const selectedFamilyId = ref<string | undefined>(undefined);
const selectedMemberAId = ref<string | undefined>(undefined);
const selectedMemberBId = ref<string | undefined>(undefined);
const result = ref<RelationshipDetectionResult | null>(null); // Use the defined interface
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
        error.value = t('relationshipDetection.selectFamilyAndMembersError');
        return;
      }

      loading.value = true;
      error.value = null;
      result.value = null;

      try {
        const detectionResult = await relationshipDetectionStore.detectRelationship(
          selectedFamilyId.value,
          selectedMemberAId.value,
          selectedMemberBId.value
        );

        if (detectionResult) {
          result.value = detectionResult;
          // Only check description if result.value is not null
          if (result.value && (result.value.description === 'unknown' || result.value.description === t('relationshipDetection.noRelationshipFound'))) {
            error.value = t('relationshipDetection.noRelationshipFound');
            result.value = null; // Clear result if unknown or no relationship found
          }
        } else {
          // If detectionResult is null, it means no relationship was found or an error occurred in the store
          error.value = t('relationshipDetection.noRelationshipFound');
          result.value = null;
        }
      } catch (err: any) {
        error.value = err.message || t('relationshipDetection.genericError');
      } finally {
        loading.value = false;
      }
    };</script>

<style scoped>
/* Add any specific styles here if needed */
</style>
