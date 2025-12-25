<template>
  <v-container fluid>
    <v-card>
      <v-card-title class="text-h5 text-center">{{ t('relationshipDetection.title') }}</v-card-title>
      <v-card-text>
        <v-form @submit.prevent="detectRelationship">
          <v-row>
            <v-col cols="12">
              <FamilyAutocomplete v-model="selectedFamilyId" :label="t('relationshipDetection.familyLabel')"
                :rules="[(v: string | undefined | null) => !!v || t('relationshipDetection.familyRequired')]" clearable
                :disabled="!!props.familyId"
                required />
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6">
              <MemberAutocomplete v-model="selectedMemberAId" :label="t('relationshipDetection.memberALabel')"
                :family-id="selectedFamilyId"
                :disabled="!selectedFamilyId"
                :rules="[(v: string | undefined | null) => !!v || t('relationshipDetection.memberARequired')]" clearable
                required />
            </v-col>
            <v-col cols="12" md="6">
              <MemberAutocomplete v-model="selectedMemberBId" :label="t('relationshipDetection.memberBLabel')"
                :family-id="selectedFamilyId"
                :disabled="!selectedFamilyId"
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
            <div style="white-space: pre-wrap;">{{ result.description }}</div>
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
import { useI18n } from 'vue-i18n';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useRelationshipDetector } from '@/composables/relationship/useRelationshipDetector';

const props = defineProps<{
  familyId?: string; // Optional familyId prop
}>();

const { t } = useI18n();
const {
  state: { selectedFamilyId, selectedMemberAId, selectedMemberBId, result, loading, error },
  actions: { detectRelationship },
} = useRelationshipDetector(props.familyId); // Pass the prop here
</script>

<style scoped>
/* Add any specific styles here if needed */
</style>
