import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { BiographyResultDto, AIProviderDto, BiographyStyle } from '@/types';
import i18n from '@/plugins/i18n';

export const useAIBiographyStore = defineStore('aiBiography', () => {
  const services = (window as any)._pinia.store.services; // Access services from global Pinia instance

  const loading = ref(false);
  const error = ref<string | null>(null);
  const biographyResult = ref<BiographyResultDto | null>(null);
  const lastUserPrompt = ref<string | null>(null);
  const aiProviders = ref<AIProviderDto[]>([]);

  // Input parameters for generation
  const memberId = ref<string | null>(null);
  const style = ref<BiographyStyle>(BiographyStyle.Emotional); // Default style
  const useDBData = ref(true);
  const userPrompt = ref<string | null>(null);
  const language = ref('Vietnamese');
  const savePromptForLater = ref(false);
  const maxTokens = ref(500); // Default token limit
  const temperature = ref(0.7); // Default temperature

  const generateBiography = async () => {
    if (!memberId.value) {
      error.value = i18n.global.t('aiBiography.errors.memberIdRequired');
      return;
    }

    loading.value = true;
    error.value = null;
    biographyResult.value = null;

    try {
      const result = await services.aiBiography.generateBiography(
        memberId.value,
        style.value,
        useDBData.value,
        userPrompt.value || undefined,
        language.value,
      );

      if (result.ok) {
        biographyResult.value = result.value;
        if (savePromptForLater.value && userPrompt.value) {
          localStorage.setItem(`lastUserPrompt_${memberId.value}`, userPrompt.value);
        }
      } else {
        error.value = result.error?.message || i18n.global.t('aiBiography.errors.generationFailed');
      }
    } catch (err: any) {
      error.value = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
    } finally {
      loading.value = false;
    }
  };

  const fetchLastUserPrompt = async (id: string) => {
    loading.value = true;
    error.value = null;
    lastUserPrompt.value = null;
    try {
      const result = await services.aiBiography.getLastUserPrompt(id);
      if (result.ok) {
        lastUserPrompt.value = result.value || null;
      } else {
        error.value = result.error?.message || i18n.global.t('aiBiography.errors.fetchLastPromptFailed');
      }
    } catch (err: any) {
      error.value = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
    } finally {
      loading.value = false;
    }
  };

  const fetchAIProviders = async () => {
    loading.value = true;
    error.value = null;
    aiProviders.value = [];
    try {
      const result = await services.aiBiography.getAIProviders();
      if (result.ok) {
        aiProviders.value = result.value;
      } else {
        error.value = result.error?.message || i18n.global.t('aiBiography.errors.fetchProvidersFailed');
      }
    } catch (err: any) {
      error.value = err.message || i18n.global.t('aiBiography.errors.unexpectedError');
    } finally {
      loading.value = false;
    }
  };

  const clearForm = () => {
    userPrompt.value = null;
    biographyResult.value = null;
    style.value = BiographyStyle.Emotional;
    useDBData.value = true;
    savePromptForLater.value = false;
  };

  const useSavedPrompt = (id: string) => {
    const savedPrompt = localStorage.getItem(`lastUserPrompt_${id}`);
    if (savedPrompt) {
      userPrompt.value = savedPrompt;
    }
  };

  return {
    loading,
    error,
    biographyResult,
    lastUserPrompt,
    aiProviders,
    memberId,
    style,
    useDBData,
    userPrompt,
    language,
    savePromptForLater,
    maxTokens,
    temperature,
    generateBiography,
    fetchLastUserPrompt,
    fetchAIProviders,
    clearForm,
    useSavedPrompt,
  };
});
