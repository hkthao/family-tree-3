<template>
  <v-card elevation="0">
    <v-card-title class="text-h5 text-center">{{ t('familyMedia.addLink.title') }}</v-card-title>
    <v-card-text>
      <v-form ref="form" @submit.prevent="submit">
        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="formData.url"
              :label="t('familyMedia.addLink.form.url')"
              :rules="[rules.required, rules.url]"
              required
            ></v-text-field>
          </v-col>
          <v-col cols="12">
            <v-text-field
              v-model="formData.fileName"
              :label="t('familyMedia.addLink.form.fileName')"
              :rules="[rules.required]"
              required
            ></v-text-field>
          </v-col>
          <v-col cols="12">
            <v-select
              v-model="formData.mediaType"
              :label="t('familyMedia.addLink.form.mediaType')"
              :items="mediaTypeOptions"
              item-title="title"
              item-value="value"
              clearable
            ></v-select>
          </v-col>
          <v-col cols="12">
            <v-textarea
              v-model="formData.description"
              :label="t('familyMedia.addLink.form.description')"
              rows="3"
            ></v-textarea>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn text @click="emit('close')">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="submit" :loading="isAdding">{{ t('common.add') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useValidationRules } from '@/composables/utils/useValidationRules';
import { MediaType } from '@/types/enums';
import { useAddFamilyMediaFromUrlMutation } from '@/composables/family-media/mutations/useAddFamilyMediaFromUrlMutation'; // This will be created next
import { useGlobalSnackbar } from '@/composables';

const props = defineProps<{
  familyId: string;
}>();
const emit = defineEmits(['close', 'saved']);
const { t } = useI18n();
const { rules } = useValidationRules();
const { showSnackbar } = useGlobalSnackbar();

const form = ref<HTMLFormElement | null>(null);
const formData = ref({
  url: '',
  fileName: '',
  mediaType: undefined as MediaType | undefined,
  description: '',
});

import { getMediaTypeOptions } from '@/composables/utils/mediaTypeOptions';

const mediaTypeOptions = getMediaTypeOptions(t);

const { mutate: addFamilyMediaFromUrl, isPending: isAdding } = useAddFamilyMediaFromUrlMutation();

const submit = async () => {
  if (!form.value) return;
  const { valid } = await form.value.validate();
  if (valid) {
    const payload = {
      familyId: props.familyId,
      url: formData.value.url,
      fileName: formData.value.fileName,
      mediaType: formData.value.mediaType,
      description: formData.value.description,
    };
    addFamilyMediaFromUrl(payload, {
      onSuccess: () => {
        showSnackbar(t('familyMedia.addLink.messages.success'), 'success');
        emit('saved');
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyMedia.addLink.messages.error'), 'error');
      },
    });
  }
};
</script>

<style scoped></style>
