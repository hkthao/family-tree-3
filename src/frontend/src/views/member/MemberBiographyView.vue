<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <h2 class="text-h5 ">{{ t('aiBiography.generator.title') }}</h2>
      </v-col>

      <v-col cols="12">
        <div v-if="aiBiographyStore.currentMember">
          <h4 class="text-h6">{{ aiBiographyStore.currentMember.fullName }}</h4>
          <p><strong>{{ t('member.form.dateOfBirth') }}:</strong> {{
            formatDate(aiBiographyStore.currentMember.dateOfBirth?.toISOString()) }}</p>
          <p><strong>{{ t('member.form.gender') }}:</strong> {{ aiBiographyStore.currentMember.gender ||
            t('common.unknown') }}</p>
          <p><strong>{{ t('member.form.placeOfBirth') }}:</strong> {{ aiBiographyStore.currentMember.placeOfBirth ||
            t('common.unknown') }}</p>
        </div>
      </v-col>

      <v-col cols="6">
        <v-select v-model="aiBiographyStore.style" :items="biographyStyles" :label="t('aiBiography.input.styleLabel')"
          item-title="text" item-value="value"></v-select>
      </v-col>
      <v-col cols="6">
        <v-checkbox v-model="aiBiographyStore.generatedFromDB" :label="t('aiBiography.input.useSystemData')"
          hide-details></v-checkbox>
      </v-col>

      <v-col cols="12">
        <v-textarea v-model="state.userPrompt" :label="t('aiBiography.input.promptLabel')"
          :placeholder="t('aiBiography.input.promptPlaceholder')" :auto-grow="true" clearable counter
          @blur="v$.userPrompt.$touch()" @input="v$.userPrompt.$touch()"
          :error-messages="v$.userPrompt.$errors.map(e => e.$message as string)"></v-textarea>
      </v-col>
    </v-row>

    <v-row justify="end">
      <v-col cols="auto">
        <v-btn color="primary" class="mr-2" :loading="aiBiographyStore.loading" :disabled="v$.$invalid"
          @click="generateBiography">
          {{ t('aiBiography.input.generateButton') }}
        </v-btn>
        <v-btn color="grey" @click="clearForm">
          {{ t('aiBiography.input.clearButton') }}
        </v-btn>
      </v-col>
    </v-row>

    <v-divider class="my-6"></v-divider>

    <v-row>
      <v-col cols="12">
        <h3 class="text-h6 mb-3">{{ t('aiBiography.output.title') }}</h3>
        <div v-if="aiBiographyStore.loading" class="text-center py-4">
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
          <p class="mt-2">{{ t('aiBiography.output.loading') }}</p>
        </div>
        <v-alert v-else-if="aiBiographyStore.error" type="error" dense dismissible>
          {{ aiBiographyStore.error }}
        </v-alert>
        <div v-else-if="aiBiographyStore.biographyResult || biographyContent">
          <v-textarea v-model="displayContent" :label="t('aiBiography.output.biographyContentLabel')" auto-grow
            variant="outlined"></v-textarea>
        </div>
        <div v-else class="text-center py-4">
          <p>{{ t('aiBiography.output.noBiographyYet') }}</p>
        </div>

        <v-row justify="end" v-if="aiBiographyStore.biographyResult || biographyContent">
          <v-col cols="auto">
            <v-btn color="primary" class="mr-2" @click="saveBiography">
              {{ t('aiBiography.output.saveButton') }}
            </v-btn>
            <v-btn color="secondary" @click="regenerateBiography">
              {{ t('aiBiography.output.regenerateButton') }}
            </v-btn>
          </v-col>
        </v-row>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref, watch, computed, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAIBiographyStore } from '@/stores/ai-biography.store';
import { BiographyStyle } from '@/types';
import { useVuelidate } from '@vuelidate/core';
import { useAIBiographyRules } from '@/validations/ai-biography.validation';

const props = defineProps<{
  memberId: string;
}>();

const aiBiographyStore = useAIBiographyStore();
const { t } = useI18n();

const biographyContent = ref(''); // Managed in parent

const state = reactive({
  userPrompt: ''
});

aiBiographyStore.userPrompt = state.userPrompt;

const rules = useAIBiographyRules();

const v$ = useVuelidate(rules, state);

const biographyStyles = computed(() => [
  { text: t('aiBiography.styles.emotional'), value: BiographyStyle.Emotional },
  { text: t('aiBiography.styles.historical'), value: BiographyStyle.Historical },
  { text: t('aiBiography.styles.storytelling'), value: BiographyStyle.Storytelling },
  { text: t('aiBiography.styles.formal'), value: BiographyStyle.Formal },
  { text: t('aiBiography.styles.informal'), value: BiographyStyle.Informal },
]);

const formatDate = (dateString: string | undefined | null) => {
  if (!dateString) return t('common.unknown');
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return t('common.unknown'); // Handle invalid date strings
  return date.toLocaleDateString('en-GB'); // 'en-GB' formats as dd/MM/yyyy
};

const fetchAndSetMemberDetails = async (id: string) => {
  await aiBiographyStore.fetchMemberDetails(id);
  if (aiBiographyStore.currentMember?.biography) {
    biographyContent.value = aiBiographyStore.currentMember.biography;
  }
};

watch(
  () => props.memberId,
  (newMemberId) => {
    if (newMemberId) {
      aiBiographyStore.memberId = newMemberId as string;
      fetchAndSetMemberDetails(newMemberId as string);
    }
  },
  { immediate: true },
);

watch(
  () => aiBiographyStore.biographyResult,
  (newResult) => {
    if (newResult) {
      biographyContent.value = newResult.content;
    }
  },
);

onMounted(() => {
  if (props.memberId) {
    aiBiographyStore.memberId = props.memberId;
    fetchAndSetMemberDetails(props.memberId);
  }
});

const generateBiography = async () => {
  const result = await v$.value.$validate();
  if (!result) {
    return;
  }
  aiBiographyStore.generateBiography();
}

const clearForm = () => {
  state.userPrompt = '';
  v$.value.$reset();
  aiBiographyStore.clearForm();
}

const displayContent = computed({
  get: () => biographyContent.value,
  set: (value) => { biographyContent.value = value; },
});

const saveBiography = () => {
  if (aiBiographyStore.memberId) {
    aiBiographyStore.saveBiography(aiBiographyStore.memberId, biographyContent.value);
  }
};

const regenerateBiography = () => {
  aiBiographyStore.generateBiography();
};
</script>
