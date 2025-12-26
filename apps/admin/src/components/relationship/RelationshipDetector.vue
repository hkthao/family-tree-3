<template>
 <div>
  <v-form @submit.prevent="detectRelationship">
    <v-row>
      <v-col cols="12">
        <FamilyAutocomplete v-model="selectedFamilyId" :label="t('relationshipDetection.familyLabel')"
          :rules="validationRules.familyId" clearable
          :disabled="!!initialFamilyId"
          required />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" :md="memberColCols">
        <MemberAutocomplete v-model="selectedMemberAId" :label="t('relationshipDetection.memberALabel')"
          :family-id="selectedFamilyId"
          :disabled="!selectedFamilyId"
          :rules="validationRules.memberAId" clearable
          required />
      </v-col>
      <v-col cols="12" :md="memberColCols">
        <MemberAutocomplete v-model="selectedMemberBId" :label="t('relationshipDetection.memberBLabel')"
          :family-id="selectedFamilyId"
          :disabled="!selectedFamilyId"
          :rules="validationRules.memberBId" clearable
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
 </div>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useRelationshipDetector } from '@/composables/relationship/useRelationshipDetector';
import { useRelationshipDetectorRules } from '@/validations/relationshipDetector.validation';

const props = defineProps<{
  initialFamilyId?: string; // Optional familyId prop to initialize the detector
  narrowView?: boolean; // New prop to control the layout of member selection
}>();

const memberColCols = computed(() => (props.narrowView ? '12' : '6'));

const { t } = useI18n();
const {
  state: { selectedFamilyId, selectedMemberAId, selectedMemberBId, result, loading, error },
  actions: { detectRelationship },
} = useRelationshipDetector(props.initialFamilyId, t);

const { rules: validationRules } = useRelationshipDetectorRules();

watch(() => props.initialFamilyId, (newFamilyId) => {
  selectedFamilyId.value = newFamilyId;
  selectedMemberAId.value = undefined;
  selectedMemberBId.value = undefined;
  result.value = null;
  error.value = null;
});
</script>

<style scoped>
/* Add any specific styles here if needed */
</style>
